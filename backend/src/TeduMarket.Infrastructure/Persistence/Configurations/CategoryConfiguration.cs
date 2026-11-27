using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Course;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.CategoryId);
        builder.Property(category => category.CategoryId).HasColumnName("category_id").ValueGeneratedNever();
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(category => category.NormalizedName).HasColumnName("normalized_name").HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
        builder.HasIndex(category => category.NormalizedName).IsUnique().HasDatabaseName("ux_categories_normalized_name");
    }
}
