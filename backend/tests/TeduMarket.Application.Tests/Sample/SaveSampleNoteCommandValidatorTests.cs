using TeduMarket.Application.Sample.Commands.SaveSampleNote;
using TeduMarket.Domain.Sample;

namespace TeduMarket.Application.Tests.Sample;

public sealed class SaveSampleNoteCommandValidatorTests
{
    private readonly SaveSampleNoteCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyTitle_ReturnsError(string? title)
    {
        // Act
        var result = _validator.Validate(new SaveSampleNoteCommand(title));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleLongerThanMax_ReturnsError()
    {
        // Arrange
        var command = new SaveSampleNoteCommand(new string('a', SampleNote.TitleMaxLength + 1));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ValidTitle_ReturnsNoError()
    {
        // Act
        var result = _validator.Validate(new SaveSampleNoteCommand("Tiêu đề"));

        // Assert
        Assert.True(result.IsValid);
    }
}
