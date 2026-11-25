using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Dựng principal cho token OpenIddict từ <see cref="User"/> (sub, name, email, role) và gắn destination của từng claim.</summary>
public sealed class ConnectPrincipalFactory(UserManager<User> userManager)
{
    public async Task<ClaimsPrincipal> CreateAsync(User user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: "OpenIddict",
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
        identity.AddClaim(new Claim(Claims.Name, user.UserName ?? user.Id.ToString()));
        if (!string.IsNullOrEmpty(user.Email))
        {
            identity.AddClaim(new Claim(Claims.Email, user.Email));
        }

        foreach (var role in await userManager.GetRolesAsync(user))
        {
            identity.AddClaim(new Claim(Claims.Role, role));
        }

        var scopeList = scopes.ToImmutableArray();
        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopeList);
        principal.SetDestinations(claim => GetDestinations(claim, scopeList));
        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ImmutableArray<string> scopes)
    {
        yield return Destinations.AccessToken;

        var isIdentityTokenClaim = claim.Type switch
        {
            Claims.Name => scopes.Contains(Scopes.Profile),
            Claims.Email => scopes.Contains(Scopes.Email),
            Claims.Role => scopes.Contains(Scopes.Roles),
            _ => false,
        };
        if (isIdentityTokenClaim)
        {
            yield return Destinations.IdentityToken;
        }
    }
}
