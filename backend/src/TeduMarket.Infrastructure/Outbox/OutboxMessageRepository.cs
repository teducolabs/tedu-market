using TeduMarket.Domain.Outbox;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Outbox;

internal sealed class OutboxMessageRepository(AppDbContext dbContext) : IOutboxMessageRepository
{
    public void Add(OutboxMessage message) => dbContext.Set<OutboxMessage>().Add(message);
}
