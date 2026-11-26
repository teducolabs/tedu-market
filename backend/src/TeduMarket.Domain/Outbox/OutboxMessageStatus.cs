namespace TeduMarket.Domain.Outbox;

/// <summary>Trạng thái xử lý của <see cref="OutboxMessage"/>; lưu DB dạng chuỗi.</summary>
public enum OutboxMessageStatus
{
    Pending,
    Processed,
    Failed,
}
