using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeduMarket.Infrastructure.Persistence;

/// <summary>Chỉ dùng cho <c>dotnet ef</c>; chuỗi kết nối lấy từ biến môi trường <c>ConnectionStrings__Default</c>, không nằm trong source.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException("Thiếu biến môi trường ConnectionStrings__Default");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("migrations_history"))
            .Options;
        return new AppDbContext(options);
    }
}
