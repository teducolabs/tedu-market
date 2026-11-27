using Mediator;

namespace TeduMarket.Application.Course.Commands.ReorderLessons;

/// <summary>Admin hoặc Instructor sở hữu khóa sắp xếp lại Bài học; <paramref name="LessonIds"/> là đủ mọi bài của khóa theo thứ tự mới (FR-20).</summary>
public sealed record ReorderLessonsCommand(Guid CourseId, IReadOnlyList<Guid>? LessonIds) : ICommand<IReadOnlyList<LessonDto>>;
