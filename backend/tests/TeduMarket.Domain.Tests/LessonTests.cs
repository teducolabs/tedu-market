using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class LessonTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 27, 0, 0, 0, TimeSpan.Zero);

    private readonly Guid _courseId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInput_StartsAsNonPreviewWithTrimmedTitle()
    {
        // Arrange / Act
        var lesson = Lesson.Create(_courseId, "  Giới thiệu ", 1, _now);

        // Assert
        Assert.Equal("Giới thiệu", lesson.LessonTitle);
        Assert.Equal(1, lesson.LessonOrder);
        Assert.False(lesson.IsPreview);
        Assert.Equal(_courseId, lesson.CourseId);
        Assert.Equal(_now, lesson.CreatedAt);
        Assert.Null(lesson.UpdatedAt);
        Assert.NotEqual(Guid.Empty, lesson.LessonId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_BlankTitle_ThrowsDomainException(string? lessonTitle)
    {
        // Arrange / Act
        var act = () => Lesson.Create(_courseId, lessonTitle, 1, _now);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_TitleLongerThanLimit_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => Lesson.Create(_courseId, new string('a', Lesson.LessonTitleMaxLength + 1), 1, _now);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_TitleAtLimit_Succeeds()
    {
        // Arrange / Act
        var lesson = Lesson.Create(_courseId, new string('a', Lesson.LessonTitleMaxLength), 1, _now);

        // Assert
        Assert.Equal(Lesson.LessonTitleMaxLength, lesson.LessonTitle.Length);
    }

    [Fact]
    public void Create_EmptyCourseId_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => Lesson.Create(Guid.Empty, "A", 1, _now);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_OrderBelowOne_ThrowsDomainException(int lessonOrder)
    {
        // Arrange / Act
        var act = () => Lesson.Create(_courseId, "A", lessonOrder, _now);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Rename_ValidTitle_ChangesTitleAndStampsUpdatedAt()
    {
        // Arrange
        var lesson = Lesson.Create(_courseId, "Cũ", 1, _now);
        var later = _now.AddHours(1);

        // Act
        lesson.Rename(" Mới ", later);

        // Assert
        Assert.Equal("Mới", lesson.LessonTitle);
        Assert.Equal(later, lesson.UpdatedAt);
    }

    [Fact]
    public void Rename_BlankTitle_ThrowsAndKeepsOldTitle()
    {
        // Arrange
        var lesson = Lesson.Create(_courseId, "Cũ", 1, _now);

        // Act
        var act = () => lesson.Rename(" ", _now);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
        Assert.Equal("Cũ", lesson.LessonTitle);
    }

    [Fact]
    public void MoveTo_ValidOrder_ChangesOrderWithoutTouchingUpdatedAt()
    {
        // Arrange
        var lesson = Lesson.Create(_courseId, "A", 1, _now);

        // Act
        lesson.MoveTo(3);

        // Assert
        Assert.Equal(3, lesson.LessonOrder);
        Assert.Null(lesson.UpdatedAt);
    }

    [Fact]
    public void MoveTo_OrderBelowOne_ThrowsDomainException()
    {
        // Arrange
        var lesson = Lesson.Create(_courseId, "A", 2, _now);

        // Act
        var act = () => lesson.MoveTo(0);

        // Assert
        Assert.Equal("INVALID_LESSON", Assert.Throws<DomainException>(act).ErrorCode);
        Assert.Equal(2, lesson.LessonOrder);
    }

    [Fact]
    public void SetPreview_TrueThenFalse_TogglesFlagAndStampsUpdatedAt()
    {
        // Arrange
        var lesson = Lesson.Create(_courseId, "A", 1, _now);
        var later = _now.AddHours(1);

        // Act
        lesson.SetPreview(true, later);
        var markedAsPreview = lesson.IsPreview;
        lesson.SetPreview(false, later);

        // Assert
        Assert.True(markedAsPreview);
        Assert.False(lesson.IsPreview);
        Assert.Equal(later, lesson.UpdatedAt);
    }
}
