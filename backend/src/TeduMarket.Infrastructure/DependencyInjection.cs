using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Sample;
using TeduMarket.Infrastructure.Persistence;
using TeduMarket.Infrastructure.Sample;

namespace TeduMarket.Infrastructure;

public static class DependencyInjection
{
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
        return services;
    }
}
