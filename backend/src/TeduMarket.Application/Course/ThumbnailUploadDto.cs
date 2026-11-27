namespace TeduMarket.Application.Course;

/// <param name="UploadUrl">Url PUT đã ký; client gửi thân file kèm đúng header <c>Content-Type</c>.</param>
/// <param name="ObjectKey">Khóa đối tượng, gửi lại khi gắn thumbnail vào Khóa học.</param>
/// <param name="ContentType">Kiểu ảnh phải gửi trong header <c>Content-Type</c>.</param>
/// <param name="ContentLength">Số byte của file; khác số này thì kho lưu trữ từ chối.</param>
/// <param name="ExpiresAt">Thời điểm url hết hạn.</param>
public sealed record ThumbnailUploadDto(string UploadUrl, string ObjectKey, string ContentType, long ContentLength, DateTimeOffset ExpiresAt);
