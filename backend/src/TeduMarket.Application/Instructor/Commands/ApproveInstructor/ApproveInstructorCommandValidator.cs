using FluentValidation;

namespace TeduMarket.Application.Instructor.Commands.ApproveInstructor;

public sealed class ApproveInstructorCommandValidator : AbstractValidator<ApproveInstructorCommand>
{
    public ApproveInstructorCommandValidator() =>
        RuleFor(command => command.UserId).NotEmpty().WithMessage("Cần chọn người dùng");
}
