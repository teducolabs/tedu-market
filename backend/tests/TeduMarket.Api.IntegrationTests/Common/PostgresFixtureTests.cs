using Npgsql;

namespace TeduMarket.Api.IntegrationTests.Common;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class PostgresFixtureTests(PostgresFixture postgres)
{
    [Fact]
    public async Task CreateDatabase_CalledTwice_ReturnsIsolatedMigratedDatabasesOnSameServerAsync()
    {
        // Arrange
        var first = await postgres.CreateDatabaseAsync();
        var second = await postgres.CreateDatabaseAsync();
        await ExecuteAsync(first, "INSERT INTO sample_notes (sample_note_id, title, created_at) VALUES (gen_random_uuid(), 'chỉ có ở database đầu', now())");

        // Act
        var firstCount = await CountNotesAsync(first);
        var secondCount = await CountNotesAsync(second);

        // Assert
        Assert.NotEqual(first, second);
        Assert.Equal(1, firstCount);
        Assert.Equal(0, secondCount);
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> CountNotesAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT count(*) FROM sample_notes", connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
