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
    public const string LoginPath = "/account/login";

    private const string _encryptionKeyKey = "OpenIddict:EncryptionKey";
    private const string _signingKeyKey = "OpenIddict:SigningKey";
    private const int _minimumKeySizeBytes = 32;

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
                    .SetTokenEndpointUris(TokenEndpointPath);
                options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow();
                options.RequireProofKeyForCodeExchange();
                options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.Roles, Scopes.OfflineAccess);
                ConfigureKeys(options, configuration, environment);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();
                if (environment.IsDevelopment())
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        services.AddAuthorizationPolicies();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ConnectPrincipalFactory>();
        return services;
    }

    private static void ConfigureKeys(OpenIddictServerBuilder options, IConfiguration configuration, IHostEnvironment environment)
    {
        var encryptionKey = configuration[_encryptionKeyKey];
        var signingKey = configuration[_signingKeyKey];
        if (!string.IsNullOrWhiteSpace(encryptionKey) && !string.IsNullOrWhiteSpace(signingKey))
        {
            options.AddEncryptionKey(CreateKey(encryptionKey, _encryptionKeyKey));
            options.AddSigningKey(CreateKey(signingKey, _signingKeyKey));
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException($"Thiếu cấu hình {_encryptionKeyKey} và {_signingKeyKey} (chuỗi base64, tối thiểu 32 byte)");
        }

        options.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
    }

    private static SymmetricSecurityKey CreateKey(string base64Value, string configurationKey)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64Value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"{configurationKey} phải là chuỗi base64", exception);
        }

        return bytes.Length < _minimumKeySizeBytes
            ? throw new InvalidOperationException($"{configurationKey} phải dài tối thiểu {_minimumKeySizeBytes} byte")
            : new SymmetricSecurityKey(bytes);
    }
}
