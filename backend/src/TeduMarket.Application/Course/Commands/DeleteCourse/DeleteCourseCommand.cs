using Mediator;

namespace TeduMarket.Application.Course.Commands.DeleteCourse;

/// <summary>Admin xóa mọi khóa, Instructor xóa khóa của mình (FR-19, FR-22).</summary>
public sealed record DeleteCourseCommand(Guid CourseId) : ICommand;
