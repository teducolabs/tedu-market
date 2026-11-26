using Mediator;

namespace TeduMarket.Application.Instructor.Commands.ApproveInstructor;

/// <summary>Admin duyệt một Học viên thành Instructor: gán role và tạo Hồ sơ Instructor (FR-40).</summary>
public sealed record ApproveInstructorCommand(Guid UserId) : ICommand<InstructorApprovalDto>;
