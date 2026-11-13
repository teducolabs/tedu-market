using TeduMarket.Domain.Sample;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Sample;

internal sealed class SampleNoteRepository(AppDbContext dbContext) : ISampleNoteRepository
{
    public void Add(SampleNote sampleNote) => dbContext.Set<SampleNote>().Add(sampleNote);
}
