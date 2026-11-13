using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Sample;
using TeduMarket.Infrastructure.Health;
using TeduMarket.Infrastructure.Persistence;
using TeduMarket.Infrastructure.Sample;

namespace TeduMarket.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Tag của health check được <c>/health/ready</c> chạy.</summary>
    public const string ReadyTag = "ready";

    /// <summary>Đăng ký PostgreSQL: một NpgsqlDataSource dùng chung cho EF Core (ghi) và Dapper (đọc). Chuỗi kết nối đọc lúc dùng lần đầu.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(
            configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:Default")).Build());
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseNpgsql(provider.GetRequiredService<NpgsqlDataSource>(), npgsql => npgsql.MigrationsHistoryTable("migrations_history")));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ISampleNoteRepository, SampleNoteRepository>();
        services.AddScoped<ISampleNoteReader, SampleNoteDapperReader>();
        services.AddHttpClient(MinioHealthCheck.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(configuration["Minio:Endpoint"] ?? throw new InvalidOperationException("Thiếu cấu hình Minio:Endpoint"));
            client.Timeout = TimeSpan.FromSeconds(3);
        });
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", tags: [ReadyTag])
            .AddCheck<MinioHealthCheck>("minio", tags: [ReadyTag]);
        return services;
    }
}
