using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.OutboxMessageId);
        builder.Property(message => message.OutboxMessageId).HasColumnName("outbox_message_id").ValueGeneratedNever();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(OutboxMessage.TypeMaxLength).IsRequired();
        builder.Property(message => message.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(message => message.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        builder.Property(message => message.NextAttemptAt).HasColumnName("next_attempt_at").HasColumnType("timestamptz");
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at").HasColumnType("timestamptz");
        builder.Property(message => message.AttemptCount).HasColumnName("attempt_count");
        builder.Property(message => message.LastError).HasColumnName("last_error").HasMaxLength(OutboxMessage.LastErrorMaxLength);
        builder.HasIndex(message => new { message.Status, message.NextAttemptAt }).HasDatabaseName("ix_outbox_messages_status_next_attempt_at");
    }
}
