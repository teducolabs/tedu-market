using FluentValidation;

namespace TeduMarket.Application.Course.Commands.UpdateCourse;

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.CategoryId).MustBeCategory();
        RuleFor(command => command.Title).MustBeTitle();
        RuleFor(command => command.Description).MustBeDescription();
        RuleFor(command => command.Price).MustBePrice();
    }
}
