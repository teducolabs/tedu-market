namespace TeduMarket.Domain.Sample;

/// <summary>Thực thể mẫu kiểm chứng đường ghi EF Core và đường đọc Dapper; xóa bằng migration mới khi có module nghiệp vụ thật.</summary>
public sealed class SampleNote
{
    public const int TitleMaxLength = 100;

    private SampleNote()
    {
    }

    public Guid SampleNoteId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static SampleNote Create(string title, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > TitleMaxLength)
        {
            throw new Common.DomainException("INVALID_SAMPLE_NOTE_TITLE", $"Tiêu đề phải có từ 1 đến {TitleMaxLength} ký tự");
        }

        return new SampleNote { SampleNoteId = Guid.CreateVersion7(), Title = title, CreatedAt = createdAt };
    }
}
