using Mediator;

namespace TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;

/// <summary>Admin xóa Hồ sơ Instructor và gỡ role Instructor của chủ hồ sơ trong cùng một lần lưu (FR-18).</summary>
public sealed record DeleteInstructorProfileCommand(Guid InstructorProfileId) : ICommand;
