namespace TeduMarket.Infrastructure.Identity;

/// <summary>Cấu hình seeding Admin đầu tiên (FR-41, NFR-22), đọc từ mục <c>AdminSeed</c>.</summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    /// <summary>Bật seeding lúc API khởi động; mặc định bật.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Email tài khoản Admin seed (chủ dự án chốt).</summary>
    public string Email { get; set; } = "support@tedu.com.vn";

    /// <summary>Đường dẫn file bí mật nhận mật khẩu; rỗng thì dùng <c>secrets/secret.json</c> dưới thư mục gốc ứng dụng.</summary>
    public string? SecretFilePath { get; set; }
}
