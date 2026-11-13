using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class SampleNoteConfiguration : IEntityTypeConfiguration<SampleNote>
{
    public void Configure(EntityTypeBuilder<SampleNote> builder)
    {
        builder.ToTable("sample_notes");
        builder.HasKey(sampleNote => sampleNote.SampleNoteId);
        builder.Property(sampleNote => sampleNote.SampleNoteId).HasColumnName("sample_note_id").ValueGeneratedNever();
        builder.Property(sampleNote => sampleNote.Title).HasColumnName("title").HasMaxLength(SampleNote.TitleMaxLength).IsRequired();
        builder.Property(sampleNote => sampleNote.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
    }
}
