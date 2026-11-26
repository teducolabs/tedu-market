namespace TeduMarket.Infrastructure.Notification;

/// <summary>Cấu hình mục <c>Notification</c>.</summary>
public sealed class NotificationOptions
{
    public const string SectionName = "Notification";

    /// <summary>URL công khai của Web học viên (không bí mật), gốc của các liên kết trong email.</summary>
    public Uri StudentWebUrl { get; set; } = new("http://localhost:3000/");
}
