using FluentValidation;

namespace TeduMarket.Application.Course.Commands.DeleteCourse;

public sealed class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseCommandValidator() =>
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
}
