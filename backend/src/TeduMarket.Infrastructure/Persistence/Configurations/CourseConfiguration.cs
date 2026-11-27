using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Course;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<CourseAggregate>
{
    public void Configure(EntityTypeBuilder<CourseAggregate> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(course => course.CourseId);
        builder.Property(course => course.CourseId).HasColumnName("course_id").ValueGeneratedNever();
        builder.Property(course => course.InstructorProfileId).HasColumnName("instructor_profile_id");
        builder.Property(course => course.CategoryId).HasColumnName("category_id");
        builder.Property(course => course.Title).HasColumnName("title").HasMaxLength(CourseAggregate.TitleMaxLength).IsRequired();
        builder.Property(course => course.Slug).HasColumnName("slug").HasMaxLength(CourseAggregate.SlugMaxLength).IsRequired();
        builder.Property(course => course.Description).HasColumnName("description").HasMaxLength(CourseAggregate.DescriptionMaxLength);
        builder.Property(course => course.Price).HasColumnName("price").HasColumnType("numeric(18,0)");
        builder.Property(course => course.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(course => course.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(course => course.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
        builder.HasIndex(course => course.Slug).IsUnique().HasDatabaseName("ux_courses_slug");
        builder.HasIndex(course => course.InstructorProfileId).HasDatabaseName("ix_courses_instructor_profile_id");
        builder.HasOne<Category>().WithMany().HasForeignKey(course => course.CategoryId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_courses_categories_category_id");
    }
}
