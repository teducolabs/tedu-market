using FluentValidation;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.CategoryId).NotEmpty().WithMessage("Cần chọn Category");
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Không được để trống")
            .Must(name => name is null || name.Trim().Length <= Category.NameMaxLength)
            .WithMessage($"Tối đa {Category.NameMaxLength} ký tự");
    }
}
