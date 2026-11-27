using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Abstractions;
using TeduMarket.Infrastructure.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>
/// Host kiểm thử Authorization Server: client mẫu (public, Code + PKCE) và endpoint đăng nhập giả <c>/test/sign-in</c>
/// (trang đăng nhập thật thuộc T15/T16) để lấy cookie Identity cho bước authorize.
/// </summary>
internal sealed class IdentityTestHost : IAsyncDisposable, IDisposable
{
    /// <summary>Khóa ký RSA PKCS#8 (base64 DER) dùng chung cho mọi host test.</summary>
    public static string SharedSigningKey => _sharedSigningKey.Value;

    public const string ResendApiKey = "re_test_key";
    public const string ClientId = "sample-client";
    public const string RedirectUri = "http://localhost/callback";
    public const string PostLogoutRedirectUri = "http://localhost/signed-out";

    private static readonly Lazy<string> _sharedSigningKey = new(() =>
    {
        using var rsa = RSA.Create(2048);
        return Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
    });

    private readonly WebApplicationFactory<Program> _factory;
    private int _userCount;

    public IdentityTestHost(
        string connectionString,
        FakeGoogleBackchannel? google = null,
        Notification.FakeResendBackchannel? resend = null,
        Microsoft.Extensions.Time.Testing.FakeTimeProvider? clock = null,
        Course.FakeMinioBackchannel? minio = null)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", connectionString);
            // Khóa RSA tạm của Development mất hàng giây mỗi host trên máy chậm: đưa khóa qua cấu hình
            // (đúng đường cấu hình của production) và dùng chung một khóa ký cho cả tiến trình test.
            builder.UseSetting("OpenIddict:EncryptionKey", Convert.ToBase64String(new byte[32]));
            builder.UseSetting("OpenIddict:SigningKey", SharedSigningKey);
            // Test tự gọi OutboxProcessor từng lô; vòng thăm dò nền không được tranh việc.
            builder.UseSetting("Outbox:Enabled", "false");
            if (resend is not null)
            {
                builder.UseSetting("Resend:ApiKey", ResendApiKey);
                builder.UseSetting("Resend:FromAddress", "TEDU Market <no-reply@example.test>");
                builder.UseSetting("Notification:StudentWebUrl", "https://hoc.example.test");
            }

            if (minio is not null)
            {
                builder.UseSetting("Minio:AccessKey", Course.FakeMinioBackchannel.AccessKey);
                builder.UseSetting("Minio:SecretKey", Course.FakeMinioBackchannel.SecretKey);
            }

            if (google is not null)
            {
                builder.UseSetting("Google:ClientId", "test-google-client");
                builder.UseSetting("Google:ClientSecret", "test-google-secret");
            }

            builder.ConfigureServices(services =>
            {
                if (resend is not null)
                {
                    // Biên ngoài: HTTP tới Resend; sender thật vẫn chạy.
                    services.AddHttpClient(TeduMarket.Infrastructure.Notification.ResendEmailSender.HttpClientName)
                        .ConfigurePrimaryHttpMessageHandler(() => resend);
                }

                if (minio is not null)
                {
                    // Biên ngoài: HTTP tới MinIO; kho lưu trữ thật (ký URL, kiểm tra ảnh) vẫn chạy.
                    services.AddHttpClient(TeduMarket.Infrastructure.Course.MinioThumbnailStorage.HttpClientName)
                        .ConfigurePrimaryHttpMessageHandler(() => minio);
                }

                if (clock is not null)
                {
                    services.RemoveAll<TimeProvider>();
                    services.AddSingleton<TimeProvider>(clock);
                }

                if (google is not null)
                {
                    // Biên ngoài duy nhất được giả: HTTP tới Google (token và userinfo); handler Google thật vẫn chạy.
                    services.AddOptions<Microsoft.AspNetCore.Authentication.Google.GoogleOptions>(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme)
                        .Configure(options => options.BackchannelHttpHandler = google);
                }

                services.AddTransient<IStartupFilter, TestSignInStartupFilter>();
                // Băm mật khẩu mặc định (PBKDF2, hàng trăm nghìn vòng) làm test đăng nhập chậm; test chỉ cần băm đúng sai.
                services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1_000);
            });
        });
    }

    public IServiceProvider Services => _factory.Services;

    public HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>Client gọi qua https: cookie tương quan của đăng nhập ngoài luôn có cờ <c>secure</c>, <c>CookieContainer</c> không gửi lại qua http.</summary>
    public HttpClient CreateHttpsClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });

    public async Task RegisterSampleClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = ClientId,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
            RedirectUris = { new Uri(RedirectUri) },
            PostLogoutRedirectUris = { new Uri(PostLogoutRedirectUri) },
        };
        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Permissions.Prefixes.Scope + Scopes.Email,
            Permissions.Prefixes.Scope + Scopes.Roles,
            Permissions.Prefixes.Scope + Scopes.OfflineAccess,
        ]);
        await manager.CreateAsync(descriptor);
    }

    /// <summary>Tạo người dùng có các vai trò đã cho.</summary>
    public async Task<Guid> CreateUserAsync(params string[] roles)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var number = Interlocked.Increment(ref _userCount);
        var user = new User { UserName = $"user{number}@example.test", Email = $"user{number}@example.test" };
        Assert.True((await userManager.CreateAsync(user)).Succeeded);
        Assert.True((await userManager.AddToRolesAsync(user, roles)).Succeeded);
        return user.Id;
    }

    /// <summary>Tạo người dùng có mật khẩu (như Admin seed) để đăng nhập bằng form email + mật khẩu.</summary>
    public async Task<Guid> CreateUserWithPasswordAsync(string email, string password, params string[] roles)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = email, Email = email };
        Assert.True((await userManager.CreateAsync(user, password)).Succeeded);
        Assert.True((await userManager.AddToRolesAsync(user, roles)).Succeeded);
        return user.Id;
    }

    /// <summary>Tải trang đăng nhập rồi gửi form; trả phản hồi của POST (cookie Identity nằm trong <paramref name="client"/>).</summary>
    public static async Task<HttpResponseMessage> SubmitLoginAsync(HttpClient client, string email, string password, string returnUrl)
    {
        var page = await client.GetAsync($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();
        var match = System.Text.RegularExpressions.Regex.Match(html, "name=\"(__RequestVerificationToken)\" value=\"([^\"]*)\"");
        Assert.True(match.Success);
        return await client.PostAsync("/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            [match.Groups[1].Value] = System.Net.WebUtility.HtmlDecode(match.Groups[2].Value),
            ["email"] = email,
            ["password"] = password,
            ["returnUrl"] = returnUrl,
        }));
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        Assert.True((await userManager.DeleteAsync(user!)).Succeeded);
    }

    /// <summary>Đăng nhập cookie cho <paramref name="userId"/> trên <paramref name="client"/>.</summary>
    public static async Task SignInAsync(HttpClient client, Guid userId)
    {
        var response = await client.GetAsync($"/test/sign-in?userId={userId}");
        response.EnsureSuccessStatusCode();
    }

    public static (string Verifier, string Challenge) CreatePkcePair()
    {
        var verifier = Base64Url(RandomNumberGenerator.GetBytes(32));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        return (verifier, challenge);
    }

    public static string BuildAuthorizeUrl(string scope, string? codeChallenge)
    {
        var url = $"/connect/authorize?client_id={ClientId}&redirect_uri={Uri.EscapeDataString(RedirectUri)}"
            + $"&response_type=code&scope={Uri.EscapeDataString(scope)}&state=xyz";
        return codeChallenge is null ? url : $"{url}&code_challenge={codeChallenge}&code_challenge_method=S256";
    }

    /// <summary>Chạy authorize đã đăng nhập, trả <c>code</c> ở redirect_uri.</summary>
    public static async Task<string> AuthorizeAsync(HttpClient client, string scope, string codeChallenge)
    {
        var response = await client.GetAsync(BuildAuthorizeUrl(scope, codeChallenge));
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var query = System.Web.HttpUtility.ParseQueryString(response.Headers.Location!.Query);
        return query["code"] ?? throw new InvalidOperationException($"Không có code: {response.Headers.Location}");
    }

    public static Task<HttpResponseMessage> ExchangeCodeAsync(HttpClient client, string code, string verifier) =>
        client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = GrantTypes.AuthorizationCode,
            ["client_id"] = ClientId,
            ["code"] = code,
            ["redirect_uri"] = RedirectUri,
            ["code_verifier"] = verifier,
        }));

    public static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken) =>
        client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = GrantTypes.RefreshToken,
            ["client_id"] = ClientId,
            ["refresh_token"] = refreshToken,
        }));

    /// <summary>Đi hết luồng Code + PKCE cho <paramref name="userId"/>, trả JSON phản hồi của endpoint token.</summary>
    public async Task<JsonElement> LoginAsync(Guid userId, string scope = "openid profile email roles offline_access")
    {
        using var client = CreateClient();
        await SignInAsync(client, userId);
        var (verifier, challenge) = CreatePkcePair();
        var code = await AuthorizeAsync(client, scope, challenge);
        var response = await ExchangeCodeAsync(client, code, verifier);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).Clone();
    }

    public async Task<string> GetAccessTokenAsync(params string[] roles)
    {
        var userId = await CreateUserAsync(roles);
        var tokens = await LoginAsync(userId);
        return tokens.GetProperty("access_token").GetString()!;
    }

    public void Dispose() => _factory.Dispose();

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class TestSignInStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                if (context.Request.Path != "/test/sign-in")
                {
                    await nextMiddleware(context);
                    return;
                }

                var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
                var signInManager = context.RequestServices.GetRequiredService<SignInManager<User>>();
                var user = await userManager.FindByIdAsync(context.Request.Query["userId"].ToString());
                await signInManager.SignInAsync(user!, isPersistent: false);
                context.Response.StatusCode = 200;
            });
            next(app);
        };
    }
}
