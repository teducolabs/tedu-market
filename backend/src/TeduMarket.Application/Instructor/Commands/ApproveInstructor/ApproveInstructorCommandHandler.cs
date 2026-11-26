using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Instructor.Commands.ApproveInstructor;

public sealed class ApproveInstructorCommandHandler(
    ICurrentUser currentUser,
    IUserRoleService userRoleService,
    IInstructorProfileRepository instructorProfileRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<ApproveInstructorCommand, InstructorApprovalDto>
{
    public async ValueTask<InstructorApprovalDto> Handle(ApproveInstructorCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới được duyệt Instructor");
        }

        var account = await userRoleService.FindAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng");
        if (account.Roles.Contains(RoleNames.Instructor))
        {
            throw new DomainException("INSTRUCTOR_ALREADY_APPROVED", "Người dùng này đã là Instructor");
        }

        if (!account.Roles.Contains(RoleNames.Student))
        {
            throw new DomainException("USER_NOT_STUDENT", "Chỉ Học viên mới được duyệt thành Instructor");
        }

        // Gán role và tạo hồ sơ chỉ lưu một lần: lỗi giữa chừng thì không có role mà thiếu hồ sơ.
        await userRoleService.StageRoleAsync(account.UserId, RoleNames.Instructor, cancellationToken);
        var instructorProfile = InstructorProfile.Create(account.UserId, timeProvider.GetUtcNow());
        instructorProfileRepository.Add(instructorProfile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstructorApprovalDto(account.UserId, instructorProfile.InstructorProfileId);
    }
}
