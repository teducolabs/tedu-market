using Mediator;
using TeduMarket.Application.Common.Paging;

namespace TeduMarket.Application.Identity.Queries.ListUsers;

/// <summary>Admin xem danh sách người dùng, lọc theo vai trò (Student, Instructor, Admin) để duyệt Instructor.</summary>
public sealed record ListUsersQuery(string? Role, int PageNumber = 1, int PageSize = ListUsersQuery.DefaultPageSize)
    : IQuery<PagedResult<UserDto>>
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
