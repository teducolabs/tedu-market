using Mediator;

namespace TeduMarket.Application.Course.Commands.DeleteLesson;

/// <summary>Admin hoặc Instructor sở hữu khóa xóa Bài học; các bài sau được dồn thứ tự lên (FR-20).</summary>
public sealed record DeleteLessonCommand(Guid CourseId, Guid LessonId) : ICommand;
