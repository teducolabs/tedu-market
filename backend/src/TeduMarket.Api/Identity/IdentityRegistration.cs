using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using TeduMarket.Api.Common;
using TeduMarket.Application.Identity;
using TeduMarket.Infrastructure.Identity;
using TeduMarket.Infrastructure.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Api.Identity;

/// <summary>Đăng ký ASP.NET Core Identity + OpenIddict (Authorization Server chạy chung trong API; NFR-06).</summary>
public static class IdentityRegistration
{
    public const string AuthorizationEndpointPath = "/connect/authorize";
    public const string TokenEndpointPath = "/connect/token";
    public const string EndSessionEndpointPath = "/connect/logout";
    public const string LoginPath = "/account/login";

    private const string _encryptionKeyKey = "OpenIddict:EncryptionKey";
    private const string _signingKeyKey = "OpenIddict:SigningKey";
    private const int _minimumKeySizeBytes = 32;
    private const int _minimumRsaKeySizeBits = 2048;

    public static IServiceCollection AddIdentityServer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = LoginPath;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.ClaimsIdentity.EmailClaimType = Claims.Email;
            options.User.RequireUniqueEmail = true;
        });

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AppDbContext>().ReplaceDefaultEntities<Guid>())
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris(AuthorizationEndpointPath)
                    .SetTokenEndpointUris(TokenEndpointPath)
                    .SetEndSessionEndpointUris(EndSessionEndpointPath);
                options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow();
                options.RequireProofKeyForCodeExchange();
                options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.Roles, Scopes.OfflineAccess);
                ConfigureKeys(options, configuration, environment);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
                if (environment.IsDevelopment())
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.EnableTokenEntryValidation();
            });

        services.AddAntiforgery();
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        AddGoogle(authentication, configuration);
        services.AddAuthorizationPolicies();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ConnectPrincipalFactory>();
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        services.AddScoped<AdminSeeder>();
        services.AddHostedService<AdminSeedHostedService>();
        services.Configure<OidcClientOptions>(OidcClientOptions.AdminAppSectionName, configuration.GetSection(OidcClientOptions.AdminAppSectionName));
        services.Configure<OidcClientOptions>(OidcClientOptions.StudentWebSectionName, configuration.GetSection(OidcClientOptions.StudentWebSectionName));
        services.AddHostedService<OidcClientSeeder>();
        services.AddScoped<GoogleAccountService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<UserSessionRevoker>();
        services.AddScoped<IUserSessionRevoker>(provider => provider.GetRequiredService<UserSessionRevoker>());
        return services;
    }

    /// <summary>Đăng ký Google khi có <c>Google:ClientId</c> và <c>Google:ClientSecret</c>; thiếu thì không có đăng nhập Google (như client OIDC chưa cấu hình).</summary>
    private static void AddGoogle(AuthenticationBuilder authentication, IConfiguration configuration)
    {
        var clientId = configuration["Google:ClientId"];
        var clientSecret = configuration["Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return;
        }

        authentication.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.ClaimActions.MapJsonKey("email_verified", "email_verified");
            // Người dùng hủy ở Google hoặc Google lỗi: về callback để báo lỗi rõ ràng cho client, không tạo tài khoản.
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(context.Properties?.RedirectUri ?? "/");
                return Task.CompletedTask;
            };
        });
    }

    private static void ConfigureKeys(OpenIddictServerBuilder options, IConfiguration configuration, IHostEnvironment environment)
    {
        var encryptionKey = configuration[_encryptionKeyKey];
        var signingKey = configuration[_signingKeyKey];
        if (!string.IsNullOrWhiteSpace(encryptionKey) && !string.IsNullOrWhiteSpace(signingKey))
        {
            options.AddEncryptionKey(CreateEncryptionKey(encryptionKey));
            options.AddSigningKey(CreateSigningKey(signingKey));
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                $"Thiếu cấu hình {_encryptionKeyKey} (base64, tối thiểu {_minimumKeySizeBytes} byte) và {_signingKeyKey} (khóa riêng RSA PKCS#8, PEM hoặc base64 của DER)");
        }

        options.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
    }

    private static SymmetricSecurityKey CreateEncryptionKey(string base64Value)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64Value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"{_encryptionKeyKey} phải là chuỗi base64", exception);
        }

        return bytes.Length < _minimumKeySizeBytes
            ? throw new InvalidOperationException($"{_encryptionKeyKey} phải dài tối thiểu {_minimumKeySizeBytes} byte")
            : new SymmetricSecurityKey(bytes);
    }

    /// <summary>OpenIddict đòi khóa ký bất đối xứng: nhận khóa riêng RSA PKCS#8 dạng PEM hoặc base64 của DER.</summary>
    private static RsaSecurityKey CreateSigningKey(string value)
    {
        var rsa = RSA.Create();
        try
        {
            if (value.Contains("-----BEGIN", StringComparison.Ordinal))
            {
                rsa.ImportFromPem(value);
            }
            else
            {
                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(value), out _);
            }
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException or ArgumentException)
        {
            rsa.Dispose();
            throw new InvalidOperationException($"{_signingKeyKey} phải là khóa riêng RSA PKCS#8 (PEM hoặc base64 của DER)", exception);
        }

        return rsa.KeySize < _minimumRsaKeySizeBits
            ? throw new InvalidOperationException($"{_signingKeyKey} phải là khóa RSA tối thiểu {_minimumRsaKeySizeBits} bit")
            : new RsaSecurityKey(rsa);
    }
}
