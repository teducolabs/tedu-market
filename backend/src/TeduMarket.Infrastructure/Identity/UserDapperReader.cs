using Dapper;
using Npgsql;
using TeduMarket.Application.Common.Paging;
using TeduMarket.Application.Identity.Queries.ListUsers;

namespace TeduMarket.Infrastructure.Identity;

internal sealed class UserDapperReader(NpgsqlDataSource dataSource) : IUserReader
{
    private const string _countSql = """
        SELECT COUNT(*)
        FROM users u
        WHERE @RoleName::text IS NULL OR EXISTS (
            SELECT 1 FROM user_roles ur JOIN roles r ON r.id = ur.role_id
            WHERE ur.user_id = u.id AND r.name = @RoleName)
        """;

    private const string _listSql = """
        SELECT u.id AS UserId,
               u.email AS Email,
               COALESCE((SELECT array_agg(r.name ORDER BY r.name)
                         FROM user_roles ur JOIN roles r ON r.id = ur.role_id
                         WHERE ur.user_id = u.id), ARRAY[]::text[]) AS Roles,
               p.instructor_profile_id AS InstructorProfileId
        FROM users u
        LEFT JOIN instructor_profiles p ON p.user_id = u.id
        WHERE @RoleName::text IS NULL OR EXISTS (
            SELECT 1 FROM user_roles ur JOIN roles r ON r.id = ur.role_id
            WHERE ur.user_id = u.id AND r.name = @RoleName)
        ORDER BY u.email, u.id
        LIMIT @PageSize OFFSET @Offset
        """;

    public async Task<PagedResult<UserDto>> ListAsync(string? roleName, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(_countSql, new { RoleName = roleName }, cancellationToken: cancellationToken));
        var rows = await connection.QueryAsync<UserRow>(new CommandDefinition(
            _listSql,
            new { RoleName = roleName, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken));
        var items = rows.Select(row => new UserDto(row.UserId, row.Email, row.Roles, row.InstructorProfileId)).ToList();
        return new PagedResult<UserDto>(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>Dapper gán qua property vì Npgsql báo cột <c>text[]</c> là <see cref="Array"/>, không khớp constructor của record.</summary>
    private sealed class UserRow
    {
        public Guid UserId { get; set; }

        public string? Email { get; set; }

        public string[] Roles { get; set; } = [];

        public Guid? InstructorProfileId { get; set; }
    }
}
