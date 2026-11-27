using FluentValidation;

namespace TeduMarket.Application.Course.Commands.DeleteLesson;

public sealed class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.LessonId).NotEmpty().WithMessage("Cần chọn Bài học");
    }
}
