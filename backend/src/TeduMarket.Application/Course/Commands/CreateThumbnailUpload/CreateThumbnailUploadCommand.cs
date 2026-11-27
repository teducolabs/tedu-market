using Mediator;

namespace TeduMarket.Application.Course.Commands.CreateThumbnailUpload;

/// <summary>Admin hoặc Instructor sở hữu khóa xin url PUT để tải thumbnail thẳng lên kho lưu trữ (FR-23).</summary>
public sealed record CreateThumbnailUploadCommand(Guid CourseId, string? ContentType, long? ContentLength)
    : ICommand<ThumbnailUploadDto>;
