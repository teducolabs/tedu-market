using Mediator;

namespace TeduMarket.Application.Course.Commands.UpdateCourse;

/// <summary>Admin sửa mọi khóa, Instructor sửa khóa của mình; thay toàn bộ thông tin, giá và Category (FR-19, FR-22).</summary>
public sealed record UpdateCourseCommand(Guid CourseId, Guid CategoryId, string? Title, string? Description, decimal? Price)
    : ICommand<CourseDto>;
