namespace TeduMarket.Application.Sample.Queries.GetSampleNotes;

/// <summary>Cổng đọc phía Application; Infrastructure cài đặt bằng Dapper.</summary>
public interface ISampleNoteReader
{
    Task<IReadOnlyList<SampleNoteDto>> ListAsync(CancellationToken cancellationToken);
}
