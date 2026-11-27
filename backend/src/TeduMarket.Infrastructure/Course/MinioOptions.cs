namespace TeduMarket.Infrastructure.Course;

/// <summary>Cấu hình mục <c>Minio</c>; <see cref="AccessKey"/> và <see cref="SecretKey"/> là secret, chỉ đọc từ biến môi trường hoặc file bí mật (NFR-22).</summary>
public sealed class MinioOptions
{
    public const string SectionName = "Minio";

    /// <summary>Url MinIO API mà API gọi tới (kiểm tra sống, kiểm tra ảnh đã tải).</summary>
    public Uri Endpoint { get; set; } = new("http://localhost:9000");

    /// <summary>Url mà trình duyệt gọi tới để tải và xem ảnh; bỏ trống thì dùng <see cref="Endpoint"/>.</summary>
    public Uri? PublicEndpoint { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public string Bucket { get; set; } = "thumbnails";

    public string Region { get; set; } = "us-east-1";

    /// <summary>Thời gian sống của url tải ảnh lên, tính bằng giây.</summary>
    public int UploadUrlLifetimeSeconds { get; set; } = 300;
}
