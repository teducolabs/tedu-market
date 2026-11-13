namespace TeduMarket.Application.Sample.Queries.GetSampleNotes;

public sealed record SampleNoteDto(Guid SampleNoteId, string Title, DateTimeOffset CreatedAt);
