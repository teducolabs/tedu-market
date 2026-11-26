using Mediator;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeduMarket.Api.Common;
using TeduMarket.Application.Identity.Commands.ChangePassword;
using TeduMarket.Infrastructure.Identity;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TeduMarket.Api.Identity;

/// <summary>Đăng nhập email + mật khẩu của Admin (<c>/account/login</c>) và đổi mật khẩu (<c>/api/v1/account/change-password</c>).</summary>
public static class AccountEndpoints
{
    private const string _defaultReturnUrl = "/";
    private const string _invalidCredentialsMessage = "Email hoặc mật khẩu chưa đúng. Bạn thử lại nhé.";
    private const string _lockedOutMessage = "Tài khoản tạm khóa do đăng nhập sai nhiều lần. Bạn thử lại sau ít phút.";

    public static IEndpointRouteBuilder MapLoginPage(this IEndpointRouteBuilder app)
    {
        app.MapGet(IdentityRegistration.LoginPath, ShowLogin).ExcludeFromDescription();
        app.MapPost(IdentityRegistration.LoginPath, LoginAsync).ExcludeFromDescription();
        return app;
    }

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/account").WithTags("Account");
        group.MapPost("/change-password", async (ChangePasswordCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Admin);
        return api;
    }

    private static IResult ShowLogin(HttpContext httpContext, IAntiforgery antiforgery, [FromQuery] string? returnUrl) =>
        Page(httpContext, antiforgery, StatusCodes.Status200OK, SafeReturnUrl(returnUrl), email: null, errorMessage: null);

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        // Đọc form và kiểm tra antiforgery thủ công để token thiếu/sai trả 400 thay vì lỗi 500.
        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest();
        }

        var form = await httpContext.Request.ReadFormAsync();
        try
        {
            await antiforgery.ValidateRequestAsync(httpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest();
        }

        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var returnUrl = SafeReturnUrl(form["returnUrl"]);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return Page(httpContext, antiforgery, StatusCodes.Status400BadRequest, returnUrl, email, "Bạn cần nhập email và mật khẩu.");
        }

        var user = await userManager.FindByEmailAsync(email.Trim());
        var result = user is null
            ? IdentitySignInResult.Failed
            : await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        var message = result.IsLockedOut ? _lockedOutMessage : _invalidCredentialsMessage;
        return Page(httpContext, antiforgery, StatusCodes.Status400BadRequest, returnUrl, email, message);
    }

    private static IResult Page(HttpContext httpContext, IAntiforgery antiforgery, int statusCode, string returnUrl, string? email, string? errorMessage)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        httpContext.Response.Headers.ContentSecurityPolicy = LoginPage.ContentSecurityPolicy;
        httpContext.Response.Headers.CacheControl = "no-store";
        var html = LoginPage.Render(returnUrl, email, errorMessage, tokens.FormFieldName, tokens.RequestToken!);
        return Results.Content(html, "text/html; charset=utf-8", statusCode: statusCode);
    }

    /// <summary>Chỉ chấp nhận đường dẫn nội bộ (chống open redirect); khác thì về trang gốc.</summary>
    internal static string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && returnUrl[0] == '/' && (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'))
            ? returnUrl
            : _defaultReturnUrl;
}
