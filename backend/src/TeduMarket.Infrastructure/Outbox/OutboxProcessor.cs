using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeduMarket.Application.Common.Outbox;
using TeduMarket.Domain.Outbox;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Outbox;

/// <summary>
/// Xử lý một lô <see cref="OutboxMessage"/>: khóa các dòng đến hạn bằng <c>FOR UPDATE SKIP LOCKED</c> trong một transaction
/// (nhiều instance chạy song song không lấy trùng nhau), gọi handler của từng message trong scope riêng, rồi ghi kết quả và commit.
/// Handler đã chạy xong mà process chết trước commit thì message được giao lại, nên handler phải idempotent.
/// </summary>
public sealed partial class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    TimeProvider clock,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger)
{
    /// <summary>Xử lý một lô và trả số message đã lấy (0 nghĩa là hết việc).</summary>
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var claimScope = scopeFactory.CreateAsyncScope();
        var dbContext = claimScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var batchSize = options.Value.BatchSize;
        var messages = await dbContext.Set<OutboxMessage>()
            .FromSql($"""
                SELECT * FROM outbox_messages
                WHERE status = 'Pending' AND next_attempt_at <= {now}
                ORDER BY created_at
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await DispatchAsync(message, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages.Count;
    }

    // Một message lỗi không được dừng cả lô hay worker (conventions §4): bắt mọi exception, ghi log và đánh dấu lỗi.
#pragma warning disable CA1031
    private async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await using var handlerScope = scopeFactory.CreateAsyncScope();
            var handler = handlerScope.ServiceProvider.GetServices<IOutboxMessageHandler>()
                .SingleOrDefault(candidate => candidate.MessageType == message.Type)
                ?? throw new InvalidOperationException($"Không có handler cho loại message '{message.Type}'");
            await handler.HandleAsync(message, cancellationToken);
            message.MarkProcessed(clock.GetUtcNow());
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogHandlingFailed(exception, message.OutboxMessageId, message.Type, message.AttemptCount + 1);
            message.MarkFailed($"{exception.GetType().Name}: {exception.Message}", clock.GetUtcNow());
        }
    }
#pragma warning restore CA1031

    [LoggerMessage(Level = LogLevel.Error, Message = "Xử lý OutboxMessage {OutboxMessageId} loại {Type} lỗi ở lần thử {AttemptCount}")]
    private partial void LogHandlingFailed(Exception exception, Guid outboxMessageId, string type, int attemptCount);
}
