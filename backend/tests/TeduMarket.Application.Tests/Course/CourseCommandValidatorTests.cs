using TeduMarket.Application.Course.Commands.CreateCourse;
using TeduMarket.Application.Course.Commands.DeleteCourse;
using TeduMarket.Application.Course.Commands.UpdateCourse;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class CourseCommandValidatorTests
{
    private static readonly Guid _categoryId = Guid.NewGuid();

    [Fact]
    public void Validate_CreateWithValidInput_ReturnsValid()
    {
        // Arrange / Act
        var result = new CreateCourseCommandValidator().Validate(new CreateCourseCommand(null, _categoryId, "Tên", "Mô tả", 0m));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_CreateWithBlankTitleEmptyCategoryAndMissingPrice_ReturnsErrorOnEachField()
    {
        // Arrange / Act
        var result = new CreateCourseCommandValidator().Validate(new CreateCourseCommand(null, Guid.Empty, " ", null, null));

        // Assert
        Assert.Equal(["CategoryId", "Price", "Title"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Fact]
    public void Validate_CreateWithTooLongTitleAndDescription_ReturnsErrorOnBoth()
    {
        // Arrange
        var command = new CreateCourseCommand(
            null, _categoryId, new string('a', CourseAggregate.TitleMaxLength + 1), new string('a', CourseAggregate.DescriptionMaxLength + 1), 1m);

        // Act
        var result = new CreateCourseCommandValidator().Validate(command);

        // Assert
        Assert.Equal(["Description", "Title"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10.5)]
    public void Validate_CreateWithNegativeOrFractionalPrice_ReturnsErrorOnPrice(double price)
    {
        // Arrange / Act
        var result = new CreateCourseCommandValidator().Validate(new CreateCourseCommand(null, _categoryId, "Tên", null, (decimal)price));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "Price");
    }

    [Fact]
    public void Validate_UpdateWithValidInput_ReturnsValid()
    {
        // Arrange / Act
        var result = new UpdateCourseCommandValidator().Validate(new UpdateCourseCommand(Guid.NewGuid(), _categoryId, "Tên", null, 10m));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_UpdateWithEmptyCourseIdAndBlankTitle_ReturnsErrorOnBoth()
    {
        // Arrange / Act
        var result = new UpdateCourseCommandValidator().Validate(new UpdateCourseCommand(Guid.Empty, _categoryId, "", null, 10m));

        // Assert
        Assert.Equal(["CourseId", "Title"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Fact]
    public void Validate_DeleteWithEmptyCourseId_ReturnsErrorOnCourseId()
    {
        // Arrange / Act
        var result = new DeleteCourseCommandValidator().Validate(new DeleteCourseCommand(Guid.Empty));

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == "CourseId");
    }
}
