using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Instructor;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.ToTable("instructor_profiles");
        builder.HasKey(instructorProfile => instructorProfile.InstructorProfileId);
        builder.Property(instructorProfile => instructorProfile.InstructorProfileId).HasColumnName("instructor_profile_id").ValueGeneratedNever();
        builder.Property(instructorProfile => instructorProfile.UserId).HasColumnName("user_id");
        builder.Property(instructorProfile => instructorProfile.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.HasIndex(instructorProfile => instructorProfile.UserId).IsUnique().HasDatabaseName("ux_instructor_profiles_user_id");
        builder.HasOne<User>().WithOne().HasForeignKey<InstructorProfile>(instructorProfile => instructorProfile.UserId)
            .HasConstraintName("fk_instructor_profiles_users_user_id");
    }
}
