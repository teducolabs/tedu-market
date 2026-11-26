using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Common.Paging;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Identity.Queries.ListUsers;

public sealed class ListUsersQueryHandler(ICurrentUser currentUser, IUserReader userReader)
    : IQueryHandler<ListUsersQuery, PagedResult<UserDto>>
{
    public async ValueTask<PagedResult<UserDto>> Handle(ListUsersQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Chỉ Admin mới xem được danh sách người dùng");
        }

        return await userReader.ListAsync(query.Role, query.PageNumber, query.PageSize, cancellationToken);
    }
}
