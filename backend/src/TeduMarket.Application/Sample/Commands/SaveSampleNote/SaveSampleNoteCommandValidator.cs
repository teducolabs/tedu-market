using FluentValidation;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Application.Sample.Commands.SaveSampleNote;

public sealed class SaveSampleNoteCommandValidator : AbstractValidator<SaveSampleNoteCommand>
{
    public SaveSampleNoteCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Không được để trống")
            .MaximumLength(SampleNote.TitleMaxLength).WithMessage($"Tối đa {SampleNote.TitleMaxLength} ký tự");
    }
}
