using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeduMarket.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TeduMarket.Api.IntegrationTests.Common;

/// <summary>
/// Một container PostgreSQL 18 cho cả suite Integration. Migration chạy 1 lần trên database mẫu;
/// mỗi test class nhân bản database riêng từ database mẫu nên không dùng chung dữ liệu.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string _templateDatabase = "template_migrated";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();
    private int _databaseCount;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ExecuteAsync($"CREATE DATABASE {_templateDatabase}");

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = _templateDatabase };
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host.UseSetting("ConnectionStrings:Default", builder.ConnectionString));
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }

        // CREATE DATABASE ... TEMPLATE yêu cầu database mẫu không còn kết nối nào.
        NpgsqlConnection.ClearAllPools();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Tạo database mới đã migrate, trả chuỗi kết nối của nó.</summary>
    public async Task<string> CreateDatabaseAsync()
    {
        var name = string.Create(CultureInfo.InvariantCulture, $"test_{Interlocked.Increment(ref _databaseCount)}");
        await ExecuteAsync($"CREATE DATABASE {name} TEMPLATE {_templateDatabase}");
        return new NpgsqlConnectionStringBuilder(_container.GetConnectionString()) { Database = name }.ConnectionString;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresTestGroup : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
