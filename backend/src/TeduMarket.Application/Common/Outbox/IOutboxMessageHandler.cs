using TeduMarket.Domain.Outbox;

namespace TeduMarket.Application.Common.Outbox;

/// <summary>
/// Xử lý một loại <see cref="OutboxMessage"/>. Message có thể được giao lại sau sự cố (at-least-once)
/// nên handler phải idempotent; ném exception để báo lỗi và kích hoạt retry.
/// </summary>
public interface IOutboxMessageHandler
{
    /// <summary>Loại message handler này xử lý.</summary>
    string MessageType { get; }

    Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken);
}
