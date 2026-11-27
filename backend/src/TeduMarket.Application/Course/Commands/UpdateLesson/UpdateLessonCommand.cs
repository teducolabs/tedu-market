using Mediator;

namespace TeduMarket.Application.Course.Commands.UpdateLesson;

/// <summary>Admin hoặc Instructor sở hữu khóa đổi Tiêu đề bài học (FR-20).</summary>
public sealed record UpdateLessonCommand(Guid CourseId, Guid LessonId, string? LessonTitle) : ICommand<LessonDto>;
