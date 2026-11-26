using TeduMarket.Application.Instructor.Commands.ApproveInstructor;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class ApproveInstructorCommandValidatorTests
{
    private readonly ApproveInstructorCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidUserId_ReturnsValid()
    {
        // Arrange
        var command = new ApproveInstructorCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUserId_ReturnsError()
    {
        // Arrange
        var command = new ApproveInstructorCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ApproveInstructorCommand.UserId));
    }
}
