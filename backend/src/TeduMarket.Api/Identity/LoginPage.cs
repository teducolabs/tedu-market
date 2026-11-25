using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace TeduMarket.Api.Identity;

/// <summary>Trang đăng nhập email + mật khẩu của Admin (HTML do Authorization Server dựng; ux.md mục 3, 4, 5, 6).</summary>
public static class LoginPage
{
    private static readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Create(UnicodeRanges.All);

    private const string _style = """
        body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:#F8FAFC;color:#0F172A;font:14px/20px Inter,system-ui,sans-serif}
        main{width:100%;max-width:400px;margin:16px;padding:32px;background:#FFFFFF;border:1px solid #CBD5E1;border-radius:12px;box-sizing:border-box}
        h1{margin:0 0 24px;font-size:24px;line-height:32px}
        .field{margin-bottom:16px}
        label{display:block;margin-bottom:4px;font-weight:600}
        input{width:100%;box-sizing:border-box;padding:8px 12px;border:1px solid #475569;border-radius:8px;font:inherit;font-size:16px;line-height:24px}
        input:focus,button:focus{outline:2px solid #2563EB;outline-offset:2px}
        .error{margin:0 0 16px;padding:12px;background:#FEE2E2;color:#B91C1C;border-radius:8px}
        button{width:100%;padding:8px 16px;background:#2563EB;color:#FFFFFF;border:0;border-radius:8px;font:inherit;font-size:16px;font-weight:600;cursor:pointer}
        """;

    /// <summary>CSP cho riêng trang này: chỉ cho phép đúng khối style ở trên (băm SHA-256), không <c>unsafe-inline</c>.</summary>
    public static string ContentSecurityPolicy { get; } =
        $"default-src 'none'; style-src 'sha256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(_style)))}'; frame-ancestors 'none'; base-uri 'none'";

    public static string Render(string returnUrl, string? email, string? errorMessage, string antiforgeryFieldName, string antiforgeryToken)
    {
        static string encode(string value) => _htmlEncoder.Encode(value);
        var error = errorMessage is null
            ? string.Empty
            : $"""<p class="error" id="login-error" role="alert">{encode(errorMessage)}</p>""";
        var describedBy = errorMessage is null ? string.Empty : """ aria-describedby="login-error" """;

        return $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Đăng nhập App quản trị</title>
            <style>{_style}</style>
            </head>
            <body>
            <main>
            <h1>Đăng nhập App quản trị</h1>
            {error}
            <form method="post" action="{IdentityRegistration.LoginPath}">
            <input type="hidden" name="{encode(antiforgeryFieldName)}" value="{encode(antiforgeryToken)}">
            <input type="hidden" name="returnUrl" value="{encode(returnUrl)}">
            <div class="field">
            <label for="email">Email (bắt buộc)</label>
            <input id="email" name="email" type="email" autocomplete="username" required value="{encode(email ?? string.Empty)}"{describedBy}>
            </div>
            <div class="field">
            <label for="password">Mật khẩu (bắt buộc)</label>
            <input id="password" name="password" type="password" autocomplete="current-password" required{describedBy}>
            </div>
            <button type="submit">Đăng nhập</button>
            </form>
            </main>
            </body>
            </html>
            """;
    }
}
