using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: Đăng nhập bằng Google (Học viên, Instructor) — ux.md §3, T16, FR-03.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class GoogleLoginTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private const string _email = "hoc.vien@example.test";
    private const string _subject = "google-sub-1";

    private readonly FakeGoogleBackchannel _google = new();
    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync()
    {
        _host = new IdentityTestHost(await postgres.CreateDatabaseAsync(), _google);
        await _host.RegisterSampleClientAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    public void Dispose() => _host?.Dispose();

    [Fact]
    public async Task Authorize_GoogleFirstSignIn_CreatesExactlyOneStudentAndIssuesCodeAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();

        // Act
        var final = await SignInWithGoogleAsync(client, "code-1");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, final.StatusCode);
        Assert.Contains("code=", final.Headers.Location!.Query, StringComparison.Ordinal);
        var user = await FindUsersAsync(_email);
        Assert.Single(user);
        Assert.Equal([RoleNames.Student], await GetRolesAsync(user[0]));
        Assert.True(user[0].EmailConfirmed);
    }

    [Fact]
    public async Task Authorize_GoogleSecondSignIn_ReusesAccountAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        _google.SetProfile("code-2", _subject, _email);
        using var firstClient = Host.CreateHttpsClient();
        await SignInWithGoogleAsync(firstClient, "code-1");
        using var secondClient = Host.CreateHttpsClient();

        // Act
        var final = await SignInWithGoogleAsync(secondClient, "code-2");

        // Assert
        Assert.Contains("code=", final.Headers.Location!.Query, StringComparison.Ordinal);
        Assert.Single(await FindUsersAsync(_email));
    }

    [Fact]
    public async Task Authorize_UserCancelsAtGoogle_RedirectsClientWithAccessDeniedAndCreatesNoAccountAsync()
    {
        // Arrange
        using var client = Host.CreateHttpsClient();

        // Act
        var final = await SignInWithGoogleAsync(client, code: null);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, final.StatusCode);
        var location = final.Headers.Location!;
        Assert.StartsWith(IdentityTestHost.RedirectUri, location.OriginalString, StringComparison.Ordinal);
        var query = System.Web.HttpUtility.ParseQueryString(location.Query);
        Assert.Equal("access_denied", query["error"]);
        Assert.Equal("xyz", query["state"]);
        Assert.Empty(await FindUsersAsync(_email));
    }

    [Fact]
    public async Task Authorize_GoogleEmailNotVerified_RejectsAndCreatesNoAccountAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email, emailVerified: false);
        using var client = Host.CreateHttpsClient();

        // Act
        var final = await SignInWithGoogleAsync(client, "code-1");

        // Assert
        Assert.Contains("error=access_denied", final.Headers.Location!.Query, StringComparison.Ordinal);
        Assert.Empty(await FindUsersAsync(_email));
    }

    [Fact]
    public async Task Authorize_GoogleEmailBelongsToAdmin_RejectsWithoutLinkingAsync()
    {
        // Arrange
        await Host.CreateUserWithPasswordAsync(_email, "Mat-khau-1!", RoleNames.Admin);
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();

        // Act
        var final = await SignInWithGoogleAsync(client, "code-1");

        // Assert
        Assert.Contains("error=access_denied", final.Headers.Location!.Query, StringComparison.Ordinal);
        var users = await FindUsersAsync(_email);
        Assert.Single(users);
        Assert.Equal([RoleNames.Admin], await GetRolesAsync(users[0]));
    }

    [Fact]
    public async Task GoogleStart_ReturnUrlIsExternal_ReturnsToRootAfterSignInAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        var start = await client.GetAsync("/account/google?returnUrl=" + Uri.EscapeDataString("https://evil.example/steal"));
        var state = System.Web.HttpUtility.ParseQueryString(start.Headers.Location!.Query)["state"];
        var providerCallback = await client.GetAsync($"/signin-google?code=code-1&state={Uri.EscapeDataString(state!)}");

        // Act
        var callback = await client.GetAsync(providerCallback.Headers.Location!.OriginalString);

        // Assert
        Assert.Equal("/", callback.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Authorize_GoogleFirstSignIn_IdTokenHasFirstSignInClaimAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();

        // Act
        var tokens = await SignInAndExchangeAsync(client, "code-1", verifier, challenge);

        // Assert
        Assert.True(ReadClaims(tokens, "id_token").TryGetProperty("first_sign_in", out var flag));
        Assert.True(flag.GetBoolean());
    }

    [Fact]
    public async Task Authorize_GoogleSecondSignIn_IdTokenHasNoFirstSignInClaimAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        _google.SetProfile("code-2", _subject, _email);
        using var firstClient = Host.CreateHttpsClient();
        var (firstVerifier, firstChallenge) = IdentityTestHost.CreatePkcePair();
        await SignInAndExchangeAsync(firstClient, "code-1", firstVerifier, firstChallenge);
        using var secondClient = Host.CreateHttpsClient();
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();

        // Act
        var tokens = await SignInAndExchangeAsync(secondClient, "code-2", verifier, challenge);

        // Assert
        Assert.False(ReadClaims(tokens, "id_token").TryGetProperty("first_sign_in", out _));
    }

    [Fact]
    public async Task Authorize_SameBrowserAuthorizesAgainAfterFirstSignIn_IdTokenHasNoFirstSignInClaimAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        var (firstVerifier, firstChallenge) = IdentityTestHost.CreatePkcePair();
        await SignInAndExchangeAsync(client, "code-1", firstVerifier, firstChallenge);
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();

        // Act
        var code = await IdentityTestHost.AuthorizeAsync(client, "openid profile email roles", challenge);
        var response = await IdentityTestHost.ExchangeCodeAsync(client, code, verifier);

        // Assert
        var tokens = (await response.Content.ReadFromJsonAsync<JsonElement>()).Clone();
        Assert.False(ReadClaims(tokens, "id_token").TryGetProperty("first_sign_in", out _));
    }

    [Fact]
    public async Task TokenEndpoint_RefreshAfterFirstSignIn_IdTokenHasNoFirstSignInClaimAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();
        var tokens = await SignInAndExchangeAsync(client, "code-1", verifier, challenge);

        // Act
        var response = await IdentityTestHost.RefreshAsync(client, tokens.GetProperty("refresh_token").GetString()!);

        // Assert
        response.EnsureSuccessStatusCode();
        var refreshed = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(refreshed.TryGetProperty("id_token", out _));
        Assert.False(ReadClaims(refreshed, "id_token").TryGetProperty("first_sign_in", out _));
    }

    private static async Task<JsonElement> SignInAndExchangeAsync(HttpClient client, string code, string verifier, string challenge)
    {
        var final = await SignInWithGoogleAsync(client, code, challenge);
        var query = System.Web.HttpUtility.ParseQueryString(final.Headers.Location!.Query);
        var response = await IdentityTestHost.ExchangeCodeAsync(client, query["code"]!, verifier);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).Clone();
    }

    /// <summary>Đọc payload của JWT (không kiểm chữ ký); chỉ dùng cho id_token (JWT ký, không mã hóa).</summary>
    private static JsonElement ReadClaims(JsonElement tokens, string tokenName)
    {
        var payload = tokens.GetProperty(tokenName).GetString()!.Split('.')[1];
        var padded = payload.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
        return JsonDocument.Parse(Convert.FromBase64String(padded)).RootElement.Clone();
    }

    /// <summary>
    /// Đi từ <c>/connect/authorize</c> (Học viên chưa đăng nhập, <c>identity_provider=google</c>) qua Google rồi về callback;
    /// <paramref name="code"/> null nghĩa là người dùng hủy ở Google. Trả phản hồi của <c>/connect/authorize</c> cuối cùng
    /// (hoặc của callback khi lỗi).
    /// </summary>
    internal static async Task<HttpResponseMessage> SignInWithGoogleAsync(HttpClient client, string? code, string? codeChallenge = null)
    {
        var challenge = codeChallenge ?? IdentityTestHost.CreatePkcePair().Item2;
        var authorizeUrl = IdentityTestHost.BuildAuthorizeUrl("openid profile email roles offline_access", challenge) + "&identity_provider=google";

        var authorize = await client.GetAsync(authorizeUrl);
        var start = await client.GetAsync(authorize.Headers.Location!.OriginalString);
        var state = System.Web.HttpUtility.ParseQueryString(start.Headers.Location!.Query)["state"];
        var providerQuery = code is null
            ? $"error=access_denied&state={Uri.EscapeDataString(state!)}"
            : $"code={code}&state={Uri.EscapeDataString(state!)}";
        var providerCallback = await client.GetAsync($"/signin-google?{providerQuery}");
        var callback = await client.GetAsync(providerCallback.Headers.Location!.OriginalString);
        return callback.Headers.Location!.OriginalString.StartsWith("/connect/authorize", StringComparison.Ordinal)
            ? await client.GetAsync(callback.Headers.Location.OriginalString)
            : callback;
    }

    private async Task<List<User>> FindUsersAsync(string email)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return userManager.Users.Where(user => user.Email == email).ToList();
    }

    private async Task<IList<string>> GetRolesAsync(User user)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().GetRolesAsync(user);
    }
}
