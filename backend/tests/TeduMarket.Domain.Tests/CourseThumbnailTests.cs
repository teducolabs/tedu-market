using TeduMarket.Domain.Common;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class CourseThumbnailTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 27, 0, 0, 0, TimeSpan.Zero);

    private static CourseAggregate NewCourse() => CourseAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Tên", "slug", null, 0m, _now);

    [Fact]
    public void Create_NewCourse_HasNoThumbnail()
    {
        // Arrange / Act
        var course = NewCourse();

        // Assert
        Assert.Null(course.ThumbnailKey);
    }

    [Fact]
    public void SetThumbnail_ValidKey_StoresKeyAndTouchesUpdatedAt()
    {
        // Arrange
        var course = NewCourse();
        var later = _now.AddMinutes(5);

        // Act
        course.SetThumbnail("a/b.png", later);

        // Assert
        Assert.Equal("a/b.png", course.ThumbnailKey);
        Assert.Equal(later, course.UpdatedAt);
    }

    [Fact]
    public void SetThumbnail_CalledTwice_ReplacesPreviousKey()
    {
        // Arrange
        var course = NewCourse();
        course.SetThumbnail("old.png", _now);

        // Act
        course.SetThumbnail("new.png", _now.AddMinutes(1));

        // Assert
        Assert.Equal("new.png", course.ThumbnailKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SetThumbnail_BlankKey_ThrowsAndKeepsPreviousKey(string? key)
    {
        // Arrange
        var course = NewCourse();
        course.SetThumbnail("keep.png", _now);

        // Act
        var act = () => course.SetThumbnail(key, _now.AddMinutes(1));

        // Assert
        Assert.Throws<DomainException>(act);
        Assert.Equal("keep.png", course.ThumbnailKey);
    }

    [Fact]
    public void SetThumbnail_KeyLongerThanMax_Throws()
    {
        // Arrange
        var course = NewCourse();

        // Act
        var act = () => course.SetThumbnail(new string('a', CourseAggregate.ThumbnailKeyMaxLength + 1), _now);

        // Assert
        Assert.Throws<DomainException>(act);
        Assert.Null(course.ThumbnailKey);
    }
}
