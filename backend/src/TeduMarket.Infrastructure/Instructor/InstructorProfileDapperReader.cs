using Dapper;
using Npgsql;
using TeduMarket.Application.Instructor;

namespace TeduMarket.Infrastructure.Instructor;

internal sealed class InstructorProfileDapperReader(NpgsqlDataSource dataSource) : IInstructorProfileReader
{
    private const string _findByIdSql = """
        SELECT p.instructor_profile_id AS InstructorProfileId,
               p.user_id AS UserId,
               p.display_name AS DisplayName,
               p.biography AS Biography,
               p.avatar_url AS AvatarUrl
        FROM instructor_profiles p
        WHERE p.instructor_profile_id = @Id
        """;

    private const string _findByUserIdSql = """
        SELECT p.instructor_profile_id AS InstructorProfileId,
               p.user_id AS UserId,
               p.display_name AS DisplayName,
               p.biography AS Biography,
               p.avatar_url AS AvatarUrl
        FROM instructor_profiles p
        WHERE p.user_id = @Id
        """;

    public Task<InstructorProfileDto?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) =>
        QuerySingleAsync(_findByIdSql, instructorProfileId, cancellationToken);

    public Task<InstructorProfileDto?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        QuerySingleAsync(_findByUserIdSql, userId, cancellationToken);

    private async Task<InstructorProfileDto?> QuerySingleAsync(string sql, Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return row is null ? null : new InstructorProfileDto(row.InstructorProfileId, row.UserId, row.DisplayName, row.Biography, row.AvatarUrl);
    }

    private sealed class Row
    {
        public Guid InstructorProfileId { get; set; }

        public Guid UserId { get; set; }

        public string? DisplayName { get; set; }

        public string? Biography { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
