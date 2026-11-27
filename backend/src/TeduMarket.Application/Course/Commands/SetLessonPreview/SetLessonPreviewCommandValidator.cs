using FluentValidation;

namespace TeduMarket.Application.Course.Commands.SetLessonPreview;

public sealed class SetLessonPreviewCommandValidator : AbstractValidator<SetLessonPreviewCommand>
{
    public SetLessonPreviewCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.LessonId).NotEmpty().WithMessage("Cần chọn Bài học");
        RuleFor(command => command.IsPreview).NotNull().WithMessage("Không được để trống");
    }
}
