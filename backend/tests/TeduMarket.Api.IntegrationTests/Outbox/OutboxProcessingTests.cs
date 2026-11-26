using Microsoft.Extensions.Time.Testing;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Api.IntegrationTests.Outbox;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class OutboxProcessingTests(PostgresFixture postgres)
{
    [Fact]
    public async Task ProcessBatch_CommittedMessage_HandlesOnceAndMarksProcessedAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, new FakeTimeProvider());
        var messageId = await host.AddMessageAsync();

        // Act
        var claimed = await host.Processor.ProcessBatchAsync(CancellationToken.None);
        var claimedAgain = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        var message = await host.GetMessageAsync(messageId);
        Assert.Equal(1, claimed);
        Assert.Equal(0, claimedAgain);
        Assert.Equal([messageId], probe.Handled);
        Assert.Equal(OutboxMessageStatus.Processed, message.Status);
        Assert.NotNull(message.ProcessedAt);
    }

    [Fact]
    public async Task ProcessBatch_MessageInUncommittedTransaction_IsInvisibleUntilCommitAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, new FakeTimeProvider());
        await using var pending = await host.AddMessageInOpenTransactionAsync();

        // Act
        var claimedBeforeCommit = await host.Processor.ProcessBatchAsync(CancellationToken.None);
        await pending.CommitAsync();
        var claimedAfterCommit = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, claimedBeforeCommit);
        Assert.Equal(1, claimedAfterCommit);
        Assert.Equal([pending.OutboxMessageId], probe.Handled);
    }

    [Fact]
    public async Task ProcessBatch_MessageInRolledBackTransaction_IsNeverHandledAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, new FakeTimeProvider());
        await using var pending = await host.AddMessageInOpenTransactionAsync();
        await pending.RollbackAsync();

        // Act
        var claimed = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, claimed);
        Assert.Empty(probe.Handled);
    }

    [Fact]
    public async Task ProcessBatch_TwoInstancesRunningConcurrently_NeverHandleSameMessageTwiceAsync()
    {
        // Arrange
        var connectionString = await postgres.CreateDatabaseAsync();
        var clock = new FakeTimeProvider();
        var probeA = new OutboxProbe();
        var probeB = new OutboxProbe();
        await using var hostA = new OutboxTestHost(connectionString, probeA, clock, batchSize: 3);
        await using var hostB = new OutboxTestHost(connectionString, probeB, clock, batchSize: 3);
        for (var i = 0; i < 6; i++)
        {
            await hostA.AddMessageAsync();
        }

        var instanceAHoldsItsBatch = new TaskCompletionSource();
        var releaseInstanceA = new TaskCompletionSource();
        probeA.OnHandle = async _ =>
        {
            instanceAHoldsItsBatch.TrySetResult();
            await releaseInstanceA.Task;
        };

        // Act
        var instanceA = hostA.Processor.ProcessBatchAsync(CancellationToken.None);
        await instanceAHoldsItsBatch.Task.WaitAsync(TimeSpan.FromSeconds(30));
        var claimedByB = await hostB.Processor.ProcessBatchAsync(CancellationToken.None);
        releaseInstanceA.SetResult();
        var claimedByA = await instanceA;

        // Assert
        var handled = probeA.Handled.Concat(probeB.Handled).ToList();
        Assert.Equal(3, claimedByA);
        Assert.Equal(3, claimedByB);
        Assert.Equal(6, handled.Count);
        Assert.Equal(6, handled.Distinct().Count());
    }

    [Fact]
    public async Task ProcessBatch_HandlerThrows_RetriesWithBackoffThenSucceedsAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        var clock = new FakeTimeProvider();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, clock);
        var messageId = await host.AddMessageAsync();
        var shouldFail = true;
        probe.OnHandle = _ => shouldFail ? throw new InvalidOperationException("Resend không phản hồi") : Task.CompletedTask;

        // Act
        await host.Processor.ProcessBatchAsync(CancellationToken.None);
        var afterFailure = await host.GetMessageAsync(messageId);
        var claimedBeforeBackoffEnds = await host.Processor.ProcessBatchAsync(CancellationToken.None);
        shouldFail = false;
        clock.Advance(OutboxMessage.BaseRetryDelay);
        var claimedAfterBackoff = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        var afterRetry = await host.GetMessageAsync(messageId);
        Assert.Equal(OutboxMessageStatus.Pending, afterFailure.Status);
        Assert.Equal(1, afterFailure.AttemptCount);
        Assert.Contains("Resend không phản hồi", afterFailure.LastError, StringComparison.Ordinal);
        Assert.Equal(0, claimedBeforeBackoffEnds);
        Assert.Equal(1, claimedAfterBackoff);
        Assert.Equal(OutboxMessageStatus.Processed, afterRetry.Status);
        Assert.Equal([messageId], probe.Handled);
    }

    [Fact]
    public async Task ProcessBatch_HandlerAlwaysThrows_BecomesFailedAfterMaxAttemptsAsync()
    {
        // Arrange
        var probe = new OutboxProbe { OnHandle = _ => throw new InvalidOperationException("lỗi vĩnh viễn") };
        var clock = new FakeTimeProvider();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, clock);
        var messageId = await host.AddMessageAsync();

        // Act
        for (var attempt = 0; attempt < OutboxMessage.MaxAttemptCount; attempt++)
        {
            await host.Processor.ProcessBatchAsync(CancellationToken.None);
            clock.Advance(TimeSpan.FromHours(1));
        }

        var claimedAfterFailed = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        var message = await host.GetMessageAsync(messageId);
        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(OutboxMessage.MaxAttemptCount, message.AttemptCount);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, claimedAfterFailed);
    }

    [Fact]
    public async Task ProcessBatch_FailingMessage_DoesNotBlockOtherMessagesInBatchAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, new FakeTimeProvider());
        var failingId = await host.AddMessageAsync("unknown.type");
        var healthyId = await host.AddMessageAsync();

        // Act
        var claimed = await host.Processor.ProcessBatchAsync(CancellationToken.None);

        // Assert
        var failing = await host.GetMessageAsync(failingId);
        var healthy = await host.GetMessageAsync(healthyId);
        Assert.Equal(2, claimed);
        Assert.Equal(1, failing.AttemptCount);
        Assert.Contains("unknown.type", failing.LastError, StringComparison.Ordinal);
        Assert.Equal(OutboxMessageStatus.Processed, healthy.Status);
    }

    [Fact]
    public async Task BackgroundService_PollingEnabled_ProcessesCommittedMessageWithoutManualCallAsync()
    {
        // Arrange
        var probe = new OutboxProbe();
        var handled = new TaskCompletionSource<Guid>();
        probe.OnHandle = message =>
        {
            handled.TrySetResult(message.OutboxMessageId);
            return Task.CompletedTask;
        };
        await using var host = new OutboxTestHost(await postgres.CreateDatabaseAsync(), probe, new FakeTimeProvider(), polling: true);
        var messageId = await host.AddMessageAsync();

        // Act
        var handledId = await handled.Task.WaitAsync(TimeSpan.FromSeconds(30));

        // Assert
        Assert.Equal(messageId, handledId);
    }
}
