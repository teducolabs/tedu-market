namespace TeduMarket.Application.Course;

/// <summary>Cổng tới kho lưu trữ thumbnail (MinIO); Infrastructure cài đặt (FR-23).</summary>
public interface IThumbnailStorage
{
    /// <summary>Cấp URL PUT có hạn; chữ ký buộc đúng <paramref name="contentType"/> và <paramref name="contentLength"/>.</summary>
    ThumbnailUploadTicket CreateUpload(string objectKey, string contentType, long contentLength);

    /// <summary>Đối tượng đã có trong kho lưu trữ chưa.</summary>
    Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>Xóa đối tượng không còn dùng; cố gắng hết sức, lỗi kho lưu trữ chỉ được ghi log và không làm hỏng thao tác đã lưu.</summary>
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>Url công khai để hiển thị thumbnail.</summary>
    string GetPublicUrl(string objectKey);
}

/// <param name="UploadUrl">Url PUT đã ký.</param>
/// <param name="ExpiresAt">Thời điểm url hết hạn.</param>
public sealed record ThumbnailUploadTicket(string UploadUrl, DateTimeOffset ExpiresAt);
