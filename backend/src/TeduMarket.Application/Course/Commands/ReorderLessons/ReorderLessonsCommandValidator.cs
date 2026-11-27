using FluentValidation;

namespace TeduMarket.Application.Course.Commands.ReorderLessons;

public sealed class ReorderLessonsCommandValidator : AbstractValidator<ReorderLessonsCommand>
{
    public ReorderLessonsCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.LessonIds).NotNull().WithMessage("Không được để trống");
    }
}
