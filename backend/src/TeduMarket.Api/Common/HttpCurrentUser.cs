using OpenIddict.Abstractions;
using TeduMarket.Application.Identity;

namespace TeduMarket.Api.Common;

/// <summary>Đọc người dùng từ token Bearer của request hiện tại.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.GetClaim(OpenIddictConstants.Claims.Subject), out var userId)
            ? userId
            : null;

    public bool IsInRole(string roleName) => httpContextAccessor.HttpContext?.User.IsInRole(roleName) ?? false;
}
