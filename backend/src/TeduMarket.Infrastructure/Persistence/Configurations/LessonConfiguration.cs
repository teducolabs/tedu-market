using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Course;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");
        builder.HasKey(lesson => lesson.LessonId);
        builder.Property(lesson => lesson.LessonId).HasColumnName("lesson_id").ValueGeneratedNever();
        builder.Property(lesson => lesson.CourseId).HasColumnName("course_id");
        builder.Property(lesson => lesson.LessonTitle).HasColumnName("lesson_title").HasMaxLength(Lesson.LessonTitleMaxLength).IsRequired();
        builder.Property(lesson => lesson.LessonOrder).HasColumnName("lesson_order");
        builder.Property(lesson => lesson.IsPreview).HasColumnName("is_preview");
        builder.Property(lesson => lesson.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(lesson => lesson.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
        builder.HasIndex(lesson => new { lesson.CourseId, lesson.LessonOrder }).HasDatabaseName("ix_lessons_course_id_lesson_order");
        builder.HasOne<CourseAggregate>().WithMany().HasForeignKey(lesson => lesson.CourseId).OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_lessons_courses_course_id");
    }
}
