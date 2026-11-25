using FluentValidation;

namespace TeduMarket.Application.Identity.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public const int PasswordMaxLength = 128;

    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Không được để trống");
        RuleFor(command => command.NewPassword)
            .NotEmpty().WithMessage("Không được để trống")
            .MaximumLength(PasswordMaxLength).WithMessage($"Tối đa {PasswordMaxLength} ký tự")
            .NotEqual(command => command.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại");
    }
}
