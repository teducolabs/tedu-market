using TeduMarket.Application.Sample.Commands.CreateSample;

namespace TeduMarket.Application.Tests.Sample;

public sealed class CreateSampleCommandValidatorTests
{
    private readonly CreateSampleCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidTitle_ReturnsNoError()
    {
        // Arrange
        var command = new CreateSampleCommand("Tiêu đề");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_ReturnsTitleError(string? title)
    {
        // Arrange
        var command = new CreateSampleCommand(title);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(CreateSampleCommand.Title));
    }

    [Fact]
    public void Validate_TitleTooLong_ReturnsTitleError()
    {
        // Arrange
        var command = new CreateSampleCommand(new string('a', CreateSampleCommandValidator.TitleMaxLength + 1));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(CreateSampleCommand.Title));
    }
}
