using FluentValidation;

namespace TeduMarket.Application.Sample.Commands.CreateSample;

public sealed class CreateSampleCommandValidator : AbstractValidator<CreateSampleCommand>
{
    public const int TitleMaxLength = 100;

    public CreateSampleCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Không được để trống")
            .MaximumLength(TitleMaxLength).WithMessage($"Tối đa {TitleMaxLength} ký tự");
    }
}
