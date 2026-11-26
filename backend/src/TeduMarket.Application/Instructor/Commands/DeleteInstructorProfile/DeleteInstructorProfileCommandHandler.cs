using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;

public sealed class DeleteInstructorProfileCommandHandler(
    ICurrentUser currentUser,
    IUserRoleService userRoleService,
    IUserSessionRevoker userSessionRevoker,
    IInstructorProfileRepository instructorProfileRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteInstructorProfileCommand>
{
    public async ValueTask<Unit> Handle(DeleteInstructorProfileCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới được xóa Hồ sơ Instructor");
        }

        var instructorProfile = await instructorProfileRepository.FindAsync(command.InstructorProfileId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Hồ sơ Instructor");

        // Xóa hồ sơ và gỡ role chỉ lưu một lần: lỗi giữa chừng thì không có Instructor mất hồ sơ mà còn role.
        var account = await userRoleService.FindAsync(instructorProfile.UserId, cancellationToken);
        if (account is not null && account.Roles.Contains(RoleNames.Instructor))
        {
            await userRoleService.StageRoleRemovalAsync(account.UserId, RoleNames.Instructor, cancellationToken);
        }

        instructorProfileRepository.Remove(instructorProfile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Token cũ còn role Instructor: thu hồi để quyền mất ngay thay vì tới khi token hết hạn.
        await userSessionRevoker.RevokeAllAsync(instructorProfile.UserId, cancellationToken);
        return Unit.Value;
    }
}
