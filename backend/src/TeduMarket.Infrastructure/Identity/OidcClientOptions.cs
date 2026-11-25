namespace TeduMarket.Infrastructure.Identity;

/// <summary>Cấu hình client OIDC của một ứng dụng; đọc từ mục <c>AdminApp</c> hoặc <c>StudentWeb</c>.</summary>
public sealed class OidcClientOptions
{
    public const string AdminAppSectionName = "AdminApp";
    public const string StudentWebSectionName = "StudentWeb";

    public string[] RedirectUris { get; set; } = [];

    public string[] PostLogoutRedirectUris { get; set; } = [];

    /// <summary>Secret của client confidential (<c>student-web</c>); <c>admin-app</c> là client public nên không dùng.</summary>
    public string? OidcClientSecret { get; set; }
}
