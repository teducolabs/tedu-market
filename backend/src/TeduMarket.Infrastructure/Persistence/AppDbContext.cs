using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Common;

namespace TeduMarket.Infrastructure.Persistence;

/// <summary>DbContext phía ghi (EF Core); phía đọc dùng Dapper qua cùng NpgsqlDataSource.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
