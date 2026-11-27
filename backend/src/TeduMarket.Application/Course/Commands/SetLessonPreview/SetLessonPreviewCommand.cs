using Mediator;

namespace TeduMarket.Application.Course.Commands.SetLessonPreview;

/// <summary>Admin hoặc Instructor sở hữu khóa đánh dấu/bỏ đánh dấu Bài học xem thử (FR-45).</summary>
public sealed record SetLessonPreviewCommand(Guid CourseId, Guid LessonId, bool? IsPreview) : ICommand<LessonDto>;
