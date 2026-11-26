using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TeduMarket.Infrastructure.Outbox;

/// <summary>Thăm dò bảng Outbox theo chu kỳ (~2 giây) và xử lý từng lô cho tới khi hết việc.</summary>
public sealed partial class OutboxBackgroundService(
    OutboxProcessor processor,
    IOptions<OutboxOptions> options,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(options.Value.PollingInterval);
        do
        {
            await ProcessUntilEmptyAsync(stoppingToken);
        }
        while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    // Lỗi hạ tầng (mất kết nối DB...) không được làm chết worker: ghi log rồi thử lại ở chu kỳ sau.
#pragma warning disable CA1031
    private async Task ProcessUntilEmptyAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await processor.ProcessBatchAsync(stoppingToken) >= options.Value.BatchSize)
            {
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogProcessingFailed(exception);
        }
    }
#pragma warning restore CA1031

    [LoggerMessage(Level = LogLevel.Error, Message = "Vòng xử lý Outbox lỗi, thử lại ở chu kỳ sau")]
    private partial void LogProcessingFailed(Exception exception);
}
