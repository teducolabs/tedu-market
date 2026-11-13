using Mediator;

namespace TeduMarket.Application.Sample.Queries.GetSampleNotes;

public sealed class GetSampleNotesQueryHandler(ISampleNoteReader reader)
    : IQueryHandler<GetSampleNotesQuery, IReadOnlyList<SampleNoteDto>>
{
    public async ValueTask<IReadOnlyList<SampleNoteDto>> Handle(GetSampleNotesQuery query, CancellationToken cancellationToken) =>
        await reader.ListAsync(cancellationToken);
}
