using FluentValidation;

namespace TeduMarket.Application.Course.Commands.CreateCourse;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(command => command.CategoryId).MustBeCategory();
        RuleFor(command => command.Title).MustBeTitle();
        RuleFor(command => command.Description).MustBeDescription();
        RuleFor(command => command.Price).MustBePrice();
    }
}
