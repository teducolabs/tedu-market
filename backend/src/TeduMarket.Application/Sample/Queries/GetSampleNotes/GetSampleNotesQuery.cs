using Mediator;

namespace TeduMarket.Application.Sample.Queries.GetSampleNotes;

public sealed record GetSampleNotesQuery : IQuery<IReadOnlyList<SampleNoteDto>>;
