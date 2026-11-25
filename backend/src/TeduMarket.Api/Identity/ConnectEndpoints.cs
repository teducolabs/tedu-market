using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TeduMarket.Infrastructure.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Api.Identity;

/// <summary>Endpoint giao thức OIDC <c>/connect/*</c> (ngoài <c>/api/v1</c>); chỉ nối OpenIddict với Identity, không có nghiệp vụ.</summary>
public static class ConnectEndpoints
{
    public static IEndpointRouteBuilder MapConnectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMethods(IdentityRegistration.AuthorizationEndpointPath, [HttpMethods.Get, HttpMethods.Post], AuthorizeAsync)
            .ExcludeFromDescription();
        app.MapPost(IdentityRegistration.TokenEndpointPath, ExchangeAsync)
            .ExcludeFromDescription();
        return app;
    }

    private static async Task<IResult> AuthorizeAsync(HttpContext httpContext, UserManager<User> userManager, ConnectPrincipalFactory principalFactory)
    {
        var request = GetRequest(httpContext);
        var authentication = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!authentication.Succeeded)
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return Forbid(Errors.LoginRequired, "Người dùng chưa đăng nhập");
            }

            var properties = new AuthenticationProperties { RedirectUri = BuildReturnUrl(httpContext) };
            return Results.Challenge(properties, [IdentityConstants.ApplicationScheme]);
        }

        var user = await userManager.GetUserAsync(authentication.Principal!);
        if (user is null)
        {
            return Forbid(Errors.AccessDenied, "Tài khoản không còn tồn tại");
        }

        var principal = await principalFactory.CreateAsync(user, request.GetScopes());
        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeAsync(HttpContext httpContext, UserManager<User> userManager, SignInManager<User> signInManager, ConnectPrincipalFactory principalFactory)
    {
        var request = GetRequest(httpContext);
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
        {
            return Forbid(Errors.UnsupportedGrantType, "Grant type không được hỗ trợ");
        }

        var authentication = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var subject = authentication.Principal?.GetClaim(Claims.Subject);
        var user = subject is null ? null : await userManager.FindByIdAsync(subject);
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            return Forbid(Errors.InvalidGrant, "Tài khoản không còn được phép đăng nhập");
        }

        var principal = await principalFactory.CreateAsync(user, authentication.Principal!.GetScopes());
        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static OpenIddictRequest GetRequest(HttpContext httpContext) =>
        httpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("Không đọc được yêu cầu OpenID Connect; kiểm tra cấu hình OpenIddict");

    private static IResult Forbid(string error, string description) =>
        Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

    private static string BuildReturnUrl(HttpContext httpContext)
    {
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> parameters = httpContext.Request.HasFormContentType
            ? httpContext.Request.Form
            : httpContext.Request.Query;
        var pairs = parameters.SelectMany(pair => pair.Value.Select(value => KeyValuePair.Create<string, string?>(pair.Key, value)));
        return httpContext.Request.PathBase + httpContext.Request.Path + QueryString.Create(pairs);
    }
}
