using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

/// <summary>Gieo 3 vai trò hệ thống bằng migration (Id cố định để migration lặp lại được).</summary>
internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public static readonly Guid StudentRoleId = new("0197a3e0-0000-7000-8000-000000000001");
    public static readonly Guid InstructorRoleId = new("0197a3e0-0000-7000-8000-000000000002");
    public static readonly Guid AdminRoleId = new("0197a3e0-0000-7000-8000-000000000003");

    public void Configure(EntityTypeBuilder<Role> builder) =>
        builder.HasData(
            Create(StudentRoleId, RoleNames.Student, "0197a3e0-student-concurrency"),
            Create(InstructorRoleId, RoleNames.Instructor, "0197a3e0-instructor-concurrency"),
            Create(AdminRoleId, RoleNames.Admin, "0197a3e0-admin-concurrency"));

    private static Role Create(Guid roleId, string name, string concurrencyStamp) => new()
    {
        Id = roleId,
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = concurrencyStamp,
    };
}
