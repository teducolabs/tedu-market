using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>
/// Đăng ký client OpenIddict <c>admin-app</c> (public, PKCE) và <c>student-web</c> (confidential) lúc API khởi động;
/// redirect URI và secret đọc từ cấu hình. Chạy lại thì cập nhật client theo cấu hình hiện tại.
/// </summary>
public sealed partial class OidcClientSeeder(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<OidcClientOptions> options,
    ILogger<OidcClientSeeder> logger) : IHostedService
{
    public const string AdminAppClientId = "admin-app";
    public const string StudentWebClientId = "student-web";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var adminApp = options.Get(OidcClientOptions.AdminAppSectionName);
        var studentWeb = options.Get(OidcClientOptions.StudentWebSectionName);
        var descriptors = new List<OpenIddictApplicationDescriptor>();

        if (HasValues(adminApp.RedirectUris))
        {
            descriptors.Add(CreateDescriptor(AdminAppClientId, ClientTypes.Public, secret: null, adminApp));
        }
        else
        {
            LogClientSkipped(AdminAppClientId, "AdminApp:RedirectUris");
        }

        if (HasValues(studentWeb.RedirectUris) && !string.IsNullOrWhiteSpace(studentWeb.OidcClientSecret))
        {
            descriptors.Add(CreateDescriptor(StudentWebClientId, ClientTypes.Confidential, studentWeb.OidcClientSecret, studentWeb));
        }
        else
        {
            LogClientSkipped(StudentWebClientId, "StudentWeb:RedirectUris và StudentWeb:OidcClientSecret");
        }

        if (descriptors.Count == 0)
        {
            return;
        }

        // Chỉ chạm DB khi có client cần đăng ký (host không cấu hình client thì không cần DB lúc khởi động).
        await using var scope = scopeFactory.CreateAsyncScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        foreach (var descriptor in descriptors)
        {
            await UpsertAsync(applicationManager, descriptor, cancellationToken);
        }
    }

    private static bool HasValues(string[] uris) => uris.Any(uri => !string.IsNullOrWhiteSpace(uri));

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static OpenIddictApplicationDescriptor CreateDescriptor(string clientId, string clientType, string? secret, OidcClientOptions clientOptions)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = secret,
            ClientType = clientType,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = clientId,
        };
        foreach (var uri in clientOptions.RedirectUris.Where(uri => !string.IsNullOrWhiteSpace(uri)))
        {
            descriptor.RedirectUris.Add(new Uri(uri));
        }

        foreach (var uri in clientOptions.PostLogoutRedirectUris.Where(uri => !string.IsNullOrWhiteSpace(uri)))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
        }

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
        descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        return descriptor;
    }

    private async Task UpsertAsync(IOpenIddictApplicationManager applicationManager, OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken)
    {
        var existing = await applicationManager.FindByClientIdAsync(descriptor.ClientId!, cancellationToken);
        if (existing is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await applicationManager.UpdateAsync(existing, descriptor, cancellationToken);
        }

        LogClientRegistered(descriptor.ClientId!);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Đã đăng ký client OIDC {ClientId}")]
    private partial void LogClientRegistered(string clientId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bỏ qua client OIDC {ClientId}: thiếu cấu hình {ConfigurationKeys}")]
    private partial void LogClientSkipped(string clientId, string configurationKeys);
}
