using FluentValidation;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Không được để trống")
            .Must(name => name is null || name.Trim().Length <= Category.NameMaxLength)
            .WithMessage($"Tối đa {Category.NameMaxLength} ký tự");
}
