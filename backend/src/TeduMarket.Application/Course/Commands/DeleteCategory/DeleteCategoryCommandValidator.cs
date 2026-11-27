using FluentValidation;

namespace TeduMarket.Application.Course.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() =>
        RuleFor(command => command.CategoryId).NotEmpty().WithMessage("Cần chọn Category");
}
