using OpenIddict.Abstractions;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Thu hồi mọi token (access, refresh, code) và authorization OpenIddict của một người dùng khi đăng xuất (FR-04).</summary>
public sealed class UserSessionRevoker(IOpenIddictTokenManager tokenManager, IOpenIddictAuthorizationManager authorizationManager)
{
    public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subject = userId.ToString();
        await foreach (var token in tokenManager.FindBySubjectAsync(subject, cancellationToken))
        {
            await tokenManager.TryRevokeAsync(token, cancellationToken);
        }

        await foreach (var authorization in authorizationManager.FindBySubjectAsync(subject, cancellationToken))
        {
            await authorizationManager.TryRevokeAsync(authorization, cancellationToken);
        }
    }
}
