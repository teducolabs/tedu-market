using FluentValidation;

namespace TeduMarket.Application.Course.Commands.AddLesson;

public sealed class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.LessonTitle).MustBeLessonTitle();
    }
}
