using Dapper;
using Npgsql;
using TeduMarket.Application.Course;

namespace TeduMarket.Infrastructure.Course;

internal sealed class LessonDapperReader(NpgsqlDataSource dataSource) : ILessonReader
{
    private const string _listSql = """
        SELECT l.lesson_id AS LessonId,
               l.course_id AS CourseId,
               l.lesson_title AS LessonTitle,
               l.lesson_order AS LessonOrder,
               l.is_preview AS IsPreview
        FROM lessons l
        WHERE l.course_id = @CourseId
        ORDER BY l.lesson_order, l.created_at
        """;

    public async Task<IReadOnlyList<LessonDto>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<Row>(
            new CommandDefinition(_listSql, new { CourseId = courseId }, cancellationToken: cancellationToken));
        return [.. rows.Select(row => new LessonDto(row.LessonId, row.CourseId, row.LessonTitle, row.LessonOrder, row.IsPreview))];
    }

    private sealed class Row
    {
        public Guid LessonId { get; set; }

        public Guid CourseId { get; set; }

        public string LessonTitle { get; set; } = string.Empty;

        public int LessonOrder { get; set; }

        public bool IsPreview { get; set; }
    }
}
