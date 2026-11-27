namespace TeduMarket.Application.Course;

/// <param name="CourseId">Id Khóa học.</param>
/// <param name="InstructorProfileId">Id Hồ sơ Instructor sở hữu khóa.</param>
/// <param name="CategoryId">Id Category.</param>
/// <param name="Title">Tiêu đề khóa học.</param>
/// <param name="Slug">Slug khóa học.</param>
/// <param name="Description">Mô tả khóa học; <c>null</c> khi để trống.</param>
/// <param name="Price">Giá (đồng, số nguyên).</param>
/// <param name="Status">Tên trạng thái vòng đời (<c>Draft</c>...).</param>
/// <param name="ThumbnailUrl">Url ảnh thumbnail; <c>null</c> khi chưa có.</param>
public sealed record CourseDto(Guid CourseId, Guid InstructorProfileId, Guid CategoryId, string Title, string Slug, string? Description, decimal Price, string Status, string? ThumbnailUrl);
