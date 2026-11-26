using TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class DeleteInstructorProfileCommandValidatorTests
{
    private readonly DeleteInstructorProfileCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidProfileId_ReturnsValid()
    {
        // Arrange
        var command = new DeleteInstructorProfileCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyProfileId_ReturnsError()
    {
        // Arrange
        var command = new DeleteInstructorProfileCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteInstructorProfileCommand.InstructorProfileId));
    }
}
