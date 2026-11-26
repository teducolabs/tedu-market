using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Instructor.Queries.GetMyInstructorProfile;

public sealed class GetMyInstructorProfileQueryHandler(ICurrentUser currentUser, IInstructorProfileReader reader)
    : IQueryHandler<GetMyInstructorProfileQuery, InstructorProfileDto>
{
    public async ValueTask<InstructorProfileDto> Handle(GetMyInstructorProfileQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Instructor) || currentUser.UserId is not { } userId)
        {
            throw new ForbiddenException("Chỉ Instructor mới có Hồ sơ Instructor của mình");
        }

        return await reader.FindByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Hồ sơ Instructor");
    }
}
