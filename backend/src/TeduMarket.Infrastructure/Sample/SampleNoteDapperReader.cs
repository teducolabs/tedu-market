using Dapper;
using Npgsql;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;

namespace TeduMarket.Infrastructure.Sample;

internal sealed class SampleNoteDapperReader(NpgsqlDataSource dataSource) : ISampleNoteReader
{
    private const string _listSql = """
        SELECT sample_note_id AS SampleNoteId, title AS Title, created_at AS CreatedAt
        FROM sample_notes
        ORDER BY created_at DESC
        LIMIT 100
        """;

    public async Task<IReadOnlyList<SampleNoteDto>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SampleNoteRow>(new CommandDefinition(_listSql, cancellationToken: cancellationToken));
        return rows.Select(row => new SampleNoteDto(row.SampleNoteId, row.Title, new DateTimeOffset(row.CreatedAt, TimeSpan.Zero))).ToList();
    }

    /// <summary>Npgsql trả <c>timestamptz</c> thành <see cref="DateTime"/> UTC, nên đọc qua Row rồi đổi sang <see cref="DateTimeOffset"/>.</summary>
    private sealed record SampleNoteRow(Guid SampleNoteId, string Title, DateTime CreatedAt);
}
