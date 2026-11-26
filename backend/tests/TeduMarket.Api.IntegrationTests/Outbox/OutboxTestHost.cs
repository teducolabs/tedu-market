using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Outbox;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Outbox;
using TeduMarket.Infrastructure.Outbox;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Api.IntegrationTests.Outbox;

/// <summary>Handler giả (biên ngoài duy nhất): ghi lại message đã xử lý và cho test chèn hành vi (chặn, ném lỗi).</summary>
internal sealed class OutboxProbe
{
    public const string MessageType = "test.recorded";

    public ConcurrentQueue<Guid> Handled { get; } = new();

    public Func<OutboxMessage, Task> OnHandle { get; set; } = _ => Task.CompletedTask;
}

internal sealed class ProbeHandler(OutboxProbe probe) : IOutboxMessageHandler
{
    public string MessageType => OutboxProbe.MessageType;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await probe.OnHandle(message);
        probe.Handled.Enqueue(message.OutboxMessageId);
    }
}

/// <summary>
/// Một instance API trên database của test, mặc định tắt vòng thăm dò để test tự gọi <see cref="Processor"/> từng lô;
/// bật <c>polling</c> để kiểm chứng BackgroundService thật.
/// </summary>
internal sealed class OutboxTestHost : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public OutboxTestHost(string connectionString, OutboxProbe probe, FakeTimeProvider clock, int batchSize = 20, bool polling = false)
    {
        Probe = probe;
        Clock = clock;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", connectionString);
            builder.UseSetting("Outbox:Enabled", polling ? "true" : "false");
            builder.UseSetting("Outbox:PollingInterval", "00:00:00.050");
            builder.UseSetting("Outbox:BatchSize", batchSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
            builder.UseSetting("OpenIddict:EncryptionKey", Convert.ToBase64String(new byte[32]));
            builder.UseSetting("OpenIddict:SigningKey", Identity.IdentityTestHost.SharedSigningKey);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
                services.AddSingleton(probe);
                services.AddScoped<IOutboxMessageHandler, ProbeHandler>();
            });
        });
    }

    public OutboxProbe Probe { get; }

    public FakeTimeProvider Clock { get; }

    public OutboxProcessor Processor => _factory.Services.GetRequiredService<OutboxProcessor>();

    /// <summary>Ghi một message qua repository + unit of work như code nghiệp vụ thật (một SaveChanges = một transaction).</summary>
    public async Task<Guid> AddMessageAsync(string type = OutboxProbe.MessageType)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var message = OutboxMessage.Create(type, "{}", Clock.GetUtcNow());
        scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>().Add(message);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        return message.OutboxMessageId;
    }

    /// <summary>Mở transaction, ghi message rồi trả transaction chưa commit để test kiểm chứng message chưa xuất hiện.</summary>
    public async Task<PendingTransaction> AddMessageInOpenTransactionAsync()
    {
        var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var transaction = await dbContext.Database.BeginTransactionAsync();
        var message = OutboxMessage.Create(OutboxProbe.MessageType, "{}", Clock.GetUtcNow());
        scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>().Add(message);
        await dbContext.SaveChangesAsync();
        return new PendingTransaction(scope, transaction, message.OutboxMessageId);
    }

    public async Task<OutboxMessage> GetMessageAsync(Guid outboxMessageId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Set<OutboxMessage>()
            .AsNoTracking()
            .SingleAsync(message => message.OutboxMessageId == outboxMessageId);
    }

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    internal sealed class PendingTransaction(AsyncServiceScope scope, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, Guid outboxMessageId) : IAsyncDisposable
    {
        public Guid OutboxMessageId { get; } = outboxMessageId;

        public Task CommitAsync() => transaction.CommitAsync();

        public Task RollbackAsync() => transaction.RollbackAsync();

        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
            await scope.DisposeAsync();
        }
    }
}
