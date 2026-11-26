namespace TeduMarket.Domain.Outbox;

/// <summary>Ghi <see cref="OutboxMessage"/> vào cùng unit of work (cùng transaction) với thay đổi nghiệp vụ gây ra nó.</summary>
public interface IOutboxMessageRepository
{
    void Add(OutboxMessage message);
}
