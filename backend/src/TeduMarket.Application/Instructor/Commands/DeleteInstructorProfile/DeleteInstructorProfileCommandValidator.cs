using FluentValidation;

namespace TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;

public sealed class DeleteInstructorProfileCommandValidator : AbstractValidator<DeleteInstructorProfileCommand>
{
    public DeleteInstructorProfileCommandValidator() =>
        RuleFor(command => command.InstructorProfileId).NotEmpty().WithMessage("Cần chọn Hồ sơ Instructor");
}
