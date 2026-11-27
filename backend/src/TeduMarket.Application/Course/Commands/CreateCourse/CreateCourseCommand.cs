using Mediator;

namespace TeduMarket.Application.Course.Commands.CreateCourse;

/// <summary>Instructor (cho chính mình) hoặc Admin (chọn Instructor) tạo Khóa học ở trạng thái Chưa đăng (FR-19).</summary>
/// <param name="InstructorProfileId">Bắt buộc với Admin; Instructor bỏ trống hoặc truyền đúng hồ sơ của mình.</param>
/// <param name="CategoryId">Id Category.</param>
/// <param name="Title">Tiêu đề khóa học.</param>
/// <param name="Description">Mô tả khóa học (tùy chọn).</param>
/// <param name="Price">Giá bằng đồng, số nguyên không âm.</param>
public sealed record CreateCourseCommand(Guid? InstructorProfileId, Guid CategoryId, string? Title, string? Description, decimal? Price)
    : ICommand<CourseDto>;
