namespace TeduMarket.Infrastructure.Notification;

/// <summary>Cấu hình mục <c>Resend</c>; <see cref="ApiKey"/> là secret, chỉ đọc từ biến môi trường hoặc file bí mật (NFR-22).</summary>
public sealed class ResendOptions
{
    public const string SectionName = "Resend";

    public string? ApiKey { get; set; }

    /// <summary>Địa chỉ người gửi, ví dụ <c>TEDU Market &lt;no-reply@domain&gt;</c>; domain phải được xác minh ở Resend.</summary>
    public string? FromAddress { get; set; }

    public Uri BaseUrl { get; set; } = new("https://api.resend.com/");
}
