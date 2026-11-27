using TeduMarket.Application.Course.Commands.CreateCategory;
using TeduMarket.Application.Course.Commands.DeleteCategory;
using TeduMarket.Application.Course.Commands.UpdateCategory;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class CategoryCommandValidatorTests
{
    [Fact]
    public void Validate_CreateWithName_ReturnsValid()
    {
        // Arrange / Act
        var result = new CreateCategoryCommandValidator().Validate(new CreateCategoryCommand("Lập trình"));

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validate_CreateWithBlankName_ReturnsErrorOnName(string? name)
    {
        // Arrange / Act
        var result = new CreateCategoryCommandValidator().Validate(new CreateCategoryCommand(name));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_CreateWithTooLongName_ReturnsErrorOnName()
    {
        // Arrange / Act
        var result = new CreateCategoryCommandValidator().Validate(new CreateCategoryCommand(new string('a', Category.NameMaxLength + 1)));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_UpdateValid_ReturnsValid()
    {
        // Arrange / Act
        var result = new UpdateCategoryCommandValidator().Validate(new UpdateCategoryCommand(Guid.NewGuid(), "Mới"));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_UpdateWithEmptyIdAndBlankName_ReturnsBothErrors()
    {
        // Arrange / Act
        var result = new UpdateCategoryCommandValidator().Validate(new UpdateCategoryCommand(Guid.Empty, " "));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "CategoryId");
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_UpdateWithTooLongName_ReturnsErrorOnName()
    {
        // Arrange / Act
        var result = new UpdateCategoryCommandValidator().Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), new string('a', Category.NameMaxLength + 1)));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_DeleteWithEmptyId_ReturnsErrorOnCategoryId()
    {
        // Arrange / Act
        var result = new DeleteCategoryCommandValidator().Validate(new DeleteCategoryCommand(Guid.Empty));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "CategoryId");
    }

    [Fact]
    public void Validate_DeleteWithId_ReturnsValid()
    {
        // Arrange / Act
        var result = new DeleteCategoryCommandValidator().Validate(new DeleteCategoryCommand(Guid.NewGuid()));

        // Assert
        Assert.True(result.IsValid);
    }
}
