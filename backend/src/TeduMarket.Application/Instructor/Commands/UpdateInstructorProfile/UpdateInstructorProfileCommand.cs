using Mediator;

namespace TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;

/// <summary>Admin sửa mọi Hồ sơ Instructor (FR-18); Instructor chỉ sửa hồ sơ của mình (FR-43). Thay toàn bộ nội dung hiển thị.</summary>
public sealed record UpdateInstructorProfileCommand(Guid InstructorProfileId, string? DisplayName, string? Biography, string? AvatarUrl)
    : ICommand<InstructorProfileDto>;
