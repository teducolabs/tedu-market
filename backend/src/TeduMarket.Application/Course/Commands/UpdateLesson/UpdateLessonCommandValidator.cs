using FluentValidation;

namespace TeduMarket.Application.Course.Commands.UpdateLesson;

public sealed class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.LessonId).NotEmpty().WithMessage("Cần chọn Bài học");
        RuleFor(command => command.LessonTitle).MustBeLessonTitle();
    }
}
