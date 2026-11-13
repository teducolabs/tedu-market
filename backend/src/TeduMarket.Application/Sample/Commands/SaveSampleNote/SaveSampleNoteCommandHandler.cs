using Mediator;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Application.Sample.Commands.SaveSampleNote;

public sealed class SaveSampleNoteCommandHandler(
    ISampleNoteRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<SaveSampleNoteCommand, SampleNoteDto>
{
    public async ValueTask<SampleNoteDto> Handle(SaveSampleNoteCommand command, CancellationToken cancellationToken)
    {
        var sampleNote = SampleNote.Create(command.Title!, timeProvider.GetUtcNow());
        repository.Add(sampleNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new SampleNoteDto(sampleNote.SampleNoteId, sampleNote.Title, sampleNote.CreatedAt);
    }
}
