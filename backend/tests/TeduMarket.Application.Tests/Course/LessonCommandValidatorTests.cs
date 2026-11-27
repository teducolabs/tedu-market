using TeduMarket.Application.Course.Commands.AddLesson;
using TeduMarket.Application.Course.Commands.DeleteLesson;
using TeduMarket.Application.Course.Commands.ReorderLessons;
using TeduMarket.Application.Course.Commands.SetLessonPreview;
using TeduMarket.Application.Course.Commands.UpdateLesson;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class LessonCommandValidatorTests
{
    private static readonly Guid _id = Guid.NewGuid();

    [Fact]
    public void Validate_AddWithValidTitle_ReturnsValid()
    {
        // Arrange / Act
        var result = new AddLessonCommandValidator().Validate(new AddLessonCommand(_id, "Bài 1"));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AddWithBlankTitleAndEmptyCourse_ReturnsErrorOnEachField()
    {
        // Arrange / Act
        var result = new AddLessonCommandValidator().Validate(new AddLessonCommand(Guid.Empty, " "));

        // Assert
        Assert.Equal(["CourseId", "LessonTitle"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Fact]
    public void Validate_UpdateWithTooLongTitle_ReturnsErrorOnTitle()
    {
        // Arrange / Act
        var result = new UpdateLessonCommandValidator().Validate(
            new UpdateLessonCommand(_id, _id, new string('a', Lesson.LessonTitleMaxLength + 1)));

        // Assert
        Assert.Equal("LessonTitle", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_UpdateWithEmptyIds_ReturnsErrorOnBothIds()
    {
        // Arrange / Act
        var result = new UpdateLessonCommandValidator().Validate(new UpdateLessonCommand(Guid.Empty, Guid.Empty, "A"));

        // Assert
        Assert.Equal(["CourseId", "LessonId"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Fact]
    public void Validate_DeleteWithEmptyIds_ReturnsErrorOnBothIds()
    {
        // Arrange / Act
        var result = new DeleteLessonCommandValidator().Validate(new DeleteLessonCommand(Guid.Empty, Guid.Empty));

        // Assert
        Assert.Equal(["CourseId", "LessonId"], result.Errors.Select(error => error.PropertyName).Order());
    }

    [Fact]
    public void Validate_ReorderWithoutLessonIds_ReturnsErrorOnLessonIds()
    {
        // Arrange / Act
        var result = new ReorderLessonsCommandValidator().Validate(new ReorderLessonsCommand(_id, null));

        // Assert
        Assert.Equal("LessonIds", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_ReorderWithLessonIds_ReturnsValid()
    {
        // Arrange / Act
        var result = new ReorderLessonsCommandValidator().Validate(new ReorderLessonsCommand(_id, [_id]));

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PreviewWithoutFlag_ReturnsErrorOnIsPreview()
    {
        // Arrange / Act
        var result = new SetLessonPreviewCommandValidator().Validate(new SetLessonPreviewCommand(_id, _id, null));

        // Assert
        Assert.Equal("IsPreview", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_PreviewWithFlag_ReturnsValid()
    {
        // Arrange / Act
        var result = new SetLessonPreviewCommandValidator().Validate(new SetLessonPreviewCommand(_id, _id, false));

        // Assert
        Assert.True(result.IsValid);
    }
}
