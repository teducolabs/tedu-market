using Mediator;

namespace TeduMarket.Application.Course.Commands.SetCourseThumbnail;

/// <summary>Gắn thumbnail đã tải lên kho lưu trữ vào Khóa học (FR-23); thay thumbnail cũ nếu có.</summary>
public sealed record SetCourseThumbnailCommand(Guid CourseId, string? ObjectKey) : ICommand<CourseDto>;
