using TeduMarket.Domain.Common;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class SampleNoteTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 13, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidTitle_ReturnsNoteWithTitleAndCreatedAt()
    {
        // Arrange
        const string title = "Tiêu đề";

        // Act
        var sampleNote = SampleNote.Create(title, _now);

        // Assert
        Assert.Equal(title, sampleNote.Title);
        Assert.Equal(_now, sampleNote.CreatedAt);
        Assert.NotEqual(Guid.Empty, sampleNote.SampleNoteId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankTitle_ThrowsDomainException(string title)
    {
        // Act
        var exception = Assert.Throws<DomainException>(() => SampleNote.Create(title, _now));

        // Assert
        Assert.Equal("INVALID_SAMPLE_NOTE_TITLE", exception.ErrorCode);
    }

    [Fact]
    public void Create_TitleLongerThanMax_ThrowsDomainException()
    {
        // Arrange
        var title = new string('a', SampleNote.TitleMaxLength + 1);

        // Act
        var exception = Assert.Throws<DomainException>(() => SampleNote.Create(title, _now));

        // Assert
        Assert.Equal("INVALID_SAMPLE_NOTE_TITLE", exception.ErrorCode);
    }
}
