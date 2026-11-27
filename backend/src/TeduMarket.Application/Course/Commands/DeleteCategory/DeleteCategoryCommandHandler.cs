using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Course.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICurrentUser currentUser,
    ICategoryRepository categoryRepository,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async ValueTask<Unit> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới được xóa Category");
        }

        var category = await categoryRepository.FindAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Category");
        if (await courseRepository.ExistsInCategoryAsync(category.CategoryId, cancellationToken))
        {
            throw new DomainException("CATEGORY_IN_USE", "Category đang có Khóa học, không thể xóa");
        }

        categoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
