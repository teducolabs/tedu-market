using Mediator;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;

namespace TeduMarket.Application.Sample.Commands.SaveSampleNote;

/// <summary>Command mẫu ghi vào PostgreSQL qua EF Core.</summary>
public sealed record SaveSampleNoteCommand(string? Title) : ICommand<SampleNoteDto>;
