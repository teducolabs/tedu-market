using TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class UpdateInstructorProfileCommandValidatorTests
{
    private readonly UpdateInstructorProfileCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), "Nguyễn Văn A", "Giới thiệu", "https://cdn.example.test/a.png");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_OnlyDisplayName_ReturnsValid()
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), "A", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyProfileId_ReturnsError()
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.Empty, "A", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.InstructorProfileId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validate_BlankDisplayName_ReturnsError(string? displayName)
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), displayName, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.DisplayName));
    }

    [Fact]
    public void Validate_DisplayNameTooLong_ReturnsError()
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), new string('a', InstructorProfile.DisplayNameMaxLength + 1), null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.DisplayName));
    }

    [Fact]
    public void Validate_BiographyTooLong_ReturnsError()
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), "A", new string('a', InstructorProfile.BiographyMaxLength + 1), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.Biography));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("not a url")]
    public void Validate_AvatarUrlNotHttp_ReturnsError(string avatarUrl)
    {
        // Arrange
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), "A", null, avatarUrl);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.AvatarUrl));
    }

    [Fact]
    public void Validate_AvatarUrlTooLong_ReturnsError()
    {
        // Arrange
        var avatarUrl = "https://cdn.example.test/" + new string('a', InstructorProfile.AvatarUrlMaxLength);
        var command = new UpdateInstructorProfileCommand(Guid.NewGuid(), "A", null, avatarUrl);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateInstructorProfileCommand.AvatarUrl));
    }
}
