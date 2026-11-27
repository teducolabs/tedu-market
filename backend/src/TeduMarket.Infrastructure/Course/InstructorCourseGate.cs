using Dapper;
using Npgsql;
using TeduMarket.Application.Course;

namespace TeduMarket.Infrastructure.Course;

internal sealed class InstructorCourseGate(NpgsqlDataSource dataSource) : IInstructorCourseGate
{
    private const string _existsSql = "SELECT EXISTS (SELECT 1 FROM courses WHERE instructor_profile_id = @InstructorProfileId)";

    public async Task<bool> HasCoursesAsync(Guid instructorProfileId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(_existsSql, new { InstructorProfileId = instructorProfileId }, cancellationToken: cancellationToken));
    }
}
