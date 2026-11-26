using TeduMarket.Application.Common.Paging;

namespace TeduMarket.Application.Identity.Queries.ListUsers;

/// <summary>Cổng đọc phía Application; Infrastructure cài đặt bằng Dapper.</summary>
public interface IUserReader
{
    Task<PagedResult<UserDto>> ListAsync(string? roleName, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
