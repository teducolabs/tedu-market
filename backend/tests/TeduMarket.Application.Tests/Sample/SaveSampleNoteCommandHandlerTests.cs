using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Sample.Commands.SaveSampleNote;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Application.Tests.Sample;

[Trait("Category", "Unit")]
public sealed class SaveSampleNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_AddsNoteAndSavesChangesAsync()
    {
        // Arrange
        var repository = new FakeRepository();
        var unitOfWork = new FakeUnitOfWork();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 11, 13, 0, 0, 0, TimeSpan.Zero));
        var handler = new SaveSampleNoteCommandHandler(repository, unitOfWork, timeProvider);

        // Act
        var result = await handler.Handle(new SaveSampleNoteCommand("Tiêu đề"), CancellationToken.None);

        // Assert
        var added = Assert.Single(repository.Added);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(added.SampleNoteId, result.SampleNoteId);
        Assert.Equal("Tiêu đề", result.Title);
        Assert.Equal(timeProvider.GetUtcNow(), result.CreatedAt);
    }

    private sealed class FakeRepository : ISampleNoteRepository
    {
        public List<SampleNote> Added { get; } = [];

        public void Add(SampleNote sampleNote) => Added.Add(sampleNote);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
