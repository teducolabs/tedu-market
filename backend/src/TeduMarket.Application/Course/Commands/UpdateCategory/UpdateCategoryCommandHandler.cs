using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Course.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    ICurrentUser currentUser,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    public async ValueTask<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới được sửa Category");
        }

        var category = await categoryRepository.FindAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Category");
        await CategoryNameRules.EnsureNameAvailableAsync(categoryRepository, command.Name!, category.CategoryId, cancellationToken);
        category.Rename(command.Name, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.CategoryId, category.Name);
    }
}
