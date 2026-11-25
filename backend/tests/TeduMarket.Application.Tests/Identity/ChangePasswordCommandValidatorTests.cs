using TeduMarket.Application.Identity.Commands.ChangePassword;

namespace TeduMarket.Application.Tests.Identity;

[Trait("Category", "Unit")]
public sealed class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyCurrentPassword_ReturnsError(string? currentPassword)
    {
        // Act
        var result = _validator.Validate(new ChangePasswordCommand(currentPassword, "Mat-khau-moi-2@"));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "CurrentPassword");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyNewPassword_ReturnsError(string? newPassword)
    {
        // Act
        var result = _validator.Validate(new ChangePasswordCommand("Mat-khau-1!", newPassword));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "NewPassword");
    }

    [Fact]
    public void Validate_NewPasswordLongerThanMax_ReturnsError()
    {
        // Arrange
        var command = new ChangePasswordCommand("Mat-khau-1!", new string('a', ChangePasswordCommandValidator.PasswordMaxLength + 1));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "NewPassword");
    }

    [Fact]
    public void Validate_NewPasswordSameAsCurrent_ReturnsError()
    {
        // Act
        var result = _validator.Validate(new ChangePasswordCommand("Mat-khau-1!", "Mat-khau-1!"));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "NewPassword");
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsNoError()
    {
        // Act
        var result = _validator.Validate(new ChangePasswordCommand("Mat-khau-1!", "Mat-khau-moi-2@"));

        // Assert
        Assert.Empty(result.Errors);
    }
}
