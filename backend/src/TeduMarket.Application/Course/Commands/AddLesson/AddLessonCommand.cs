using Mediator;

namespace TeduMarket.Application.Course.Commands.AddLesson;

/// <summary>Admin hoặc Instructor sở hữu khóa thêm Bài học vào cuối khóa (FR-20).</summary>
public sealed record AddLessonCommand(Guid CourseId, string? LessonTitle) : ICommand<LessonDto>;
