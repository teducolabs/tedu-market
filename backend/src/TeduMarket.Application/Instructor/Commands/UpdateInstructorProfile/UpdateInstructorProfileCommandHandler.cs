using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;

public sealed class UpdateInstructorProfileCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileRepository instructorProfileRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateInstructorProfileCommand, InstructorProfileDto>
{
    public async ValueTask<InstructorProfileDto> Handle(UpdateInstructorProfileCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin) && !currentUser.IsInRole(RoleNames.Instructor))
        {
            throw new ForbiddenException("Chỉ Admin hoặc Instructor mới được sửa Hồ sơ Instructor");
        }

        var instructorProfile = await instructorProfileRepository.FindAsync(command.InstructorProfileId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Hồ sơ Instructor");
        OwnershipGuard.EnsureOwnerOrAdmin(currentUser, instructorProfile.UserId);

        instructorProfile.UpdateContent(command.DisplayName, command.Biography, command.AvatarUrl, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new InstructorProfileDto(
            instructorProfile.InstructorProfileId,
            instructorProfile.UserId,
            instructorProfile.DisplayName,
            instructorProfile.Biography,
            instructorProfile.AvatarUrl);
    }
}
