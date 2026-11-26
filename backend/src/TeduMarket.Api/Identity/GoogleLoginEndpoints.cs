using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OpenIddict.Abstractions;
using TeduMarket.Infrastructure.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Api.Identity;

/// <summary>
/// Đăng nhập bằng Google của Học viên và Instructor (FR-03): <c>/account/google</c> chuyển sang Google,
/// <c>/account/google/callback</c> nhận kết quả, tạo tài khoản Học viên lần đầu rồi quay lại <c>/connect/authorize</c>.
/// </summary>
public static class GoogleLoginEndpoints
{
    public const string Path = "/account/google";
    public const string CallbackPath = "/account/google/callback";

    /// <summary>Tham số tùy chọn của <c>/connect/authorize</c>: giá trị <c>google</c> chuyển thẳng sang Google thay vì trang đăng nhập Admin.</summary>
    public const string IdentityProviderParameter = "identity_provider";

    public const string GoogleValue = "google";

    private const string _incompleteMessage = "Đăng nhập chưa hoàn tất. Bạn thử lại nhé";

    public static IEndpointRouteBuilder MapGoogleLogin(this IEndpointRouteBuilder app)
    {
        app.MapGet(Path, StartAsync).ExcludeFromDescription();
        app.MapGet(CallbackPath, CallbackAsync).ExcludeFromDescription();
        return app;
    }

    private static async Task<IResult> StartAsync(
        SignInManager<User> signInManager,
        IAuthenticationSchemeProvider schemes,
        [FromQuery] string? returnUrl)
    {
        if (await schemes.GetSchemeAsync(GoogleDefaults.AuthenticationScheme) is null)
        {
            return Results.NotFound();
        }

        var callback = $"{CallbackPath}?returnUrl={Uri.EscapeDataString(AccountEndpoints.SafeReturnUrl(returnUrl))}";
        var properties = signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, callback);
        return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
    }

    private static async Task<IResult> CallbackAsync(
        HttpContext httpContext,
        SignInManager<User> signInManager,
        GoogleAccountService accountService,
        IOpenIddictApplicationManager applicationManager,
        [FromQuery] string? returnUrl)
    {
        var safeReturnUrl = AccountEndpoints.SafeReturnUrl(returnUrl);
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            // Người dùng hủy ở Google hoặc Google lỗi: không có đăng nhập ngoài để xử lý.
            return await ReportFailureAsync(safeReturnUrl, applicationManager, httpContext.RequestAborted);
        }

        var emailVerified = bool.TryParse(info.Principal.FindFirst("email_verified")?.Value, out var verified) && verified;
        var result = await accountService.FindOrCreateStudentAsync(
            info.LoginProvider,
            info.ProviderKey,
            info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            emailVerified,
            httpContext.RequestAborted);
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        if (!result.Succeeded)
        {
            return await ReportFailureAsync(safeReturnUrl, applicationManager, httpContext.RequestAborted);
        }

        if (result.Outcome == GoogleAccountOutcome.StudentCreated)
        {
            await signInManager.SignInWithClaimsAsync(
                result.User!,
                isPersistent: false,
                [new System.Security.Claims.Claim(ConnectPrincipalFactory.FirstSignInClaim, "true")]);
        }
        else
        {
            await signInManager.SignInAsync(result.User!, isPersistent: false);
        }

        return Results.LocalRedirect(safeReturnUrl);
    }

    /// <summary>
    /// Báo lỗi cho client OIDC theo chuẩn OAuth2 (<c>error=access_denied</c> kèm <c>state</c>) tại <c>redirect_uri</c> đã đăng ký,
    /// lấy từ yêu cầu authorize gốc; không xác minh được thì trả 400 chữ thường.
    /// </summary>
    private static async Task<IResult> ReportFailureAsync(string returnUrl, IOpenIddictApplicationManager applicationManager, CancellationToken cancellationToken)
    {
        var queryStart = returnUrl.IndexOf('?', StringComparison.Ordinal);
        var query = queryStart < 0 ? [] : QueryHelpers.ParseQuery(returnUrl[queryStart..]);
        var clientId = query.TryGetValue(Parameters.ClientId, out var clientValues) ? clientValues.ToString() : null;
        var redirectUri = query.TryGetValue(Parameters.RedirectUri, out var redirectValues) ? redirectValues.ToString() : null;
        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(redirectUri)
            && await applicationManager.FindByClientIdAsync(clientId, cancellationToken) is { } application
            && await applicationManager.ValidateRedirectUriAsync(application, redirectUri, cancellationToken))
        {
            var parameters = new Dictionary<string, string?>
            {
                [Parameters.Error] = Errors.AccessDenied,
                [Parameters.ErrorDescription] = _incompleteMessage,
                [Parameters.State] = query.TryGetValue(Parameters.State, out var state) ? state.ToString() : null,
            };
            return Results.Redirect(QueryHelpers.AddQueryString(redirectUri, parameters));
        }

        return Results.Text(_incompleteMessage, "text/plain; charset=utf-8", statusCode: StatusCodes.Status400BadRequest);
    }
}
