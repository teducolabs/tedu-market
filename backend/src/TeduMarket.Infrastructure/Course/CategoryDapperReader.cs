using Dapper;
using Npgsql;
using TeduMarket.Application.Course;

namespace TeduMarket.Infrastructure.Course;

internal sealed class CategoryDapperReader(NpgsqlDataSource dataSource) : ICategoryReader
{
    private const string _listSql = """
        SELECT c.category_id AS CategoryId,
               c.name AS Name
        FROM categories c
        ORDER BY c.name, c.category_id
        """;

    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<Row>(new CommandDefinition(_listSql, cancellationToken: cancellationToken));
        return [.. rows.Select(row => new CategoryDto(row.CategoryId, row.Name))];
    }

    private sealed class Row
    {
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
