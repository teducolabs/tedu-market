using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Common;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Infrastructure.Persistence;

/// <summary>DbContext phía ghi (EF Core) kèm bảng Identity và OpenIddict; phía đọc dùng Dapper qua cùng NpgsqlDataSource.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SnakeCaseNaming.Apply(builder);
    }
}
