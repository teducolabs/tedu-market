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
        categoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
