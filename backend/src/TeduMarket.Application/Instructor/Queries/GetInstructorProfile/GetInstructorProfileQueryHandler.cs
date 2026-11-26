using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Instructor.Queries.GetInstructorProfile;

public sealed class GetInstructorProfileQueryHandler(ICurrentUser currentUser, IInstructorProfileReader reader)
    : IQueryHandler<GetInstructorProfileQuery, InstructorProfileDto>
{
    public async ValueTask<InstructorProfileDto> Handle(GetInstructorProfileQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin) && !currentUser.IsInRole(RoleNames.Instructor))
        {
            throw new ForbiddenException("Chỉ Admin hoặc Instructor mới xem được Hồ sơ Instructor");
        }

        var instructorProfile = await reader.FindAsync(query.InstructorProfileId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Hồ sơ Instructor");
        OwnershipGuard.EnsureOwnerOrAdmin(currentUser, instructorProfile.UserId);
        return instructorProfile;
    }
}
