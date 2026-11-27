using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Course.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICurrentUser currentUser,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    public async ValueTask<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới được tạo Category");
        }

        await CategoryNameRules.EnsureNameAvailableAsync(categoryRepository, command.Name!, null, cancellationToken);
        var category = Category.Create(command.Name, timeProvider.GetUtcNow());
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.CategoryId, category.Name);
    }
}
