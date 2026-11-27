using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class CourseTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 27, 0, 0, 0, TimeSpan.Zero);

    private readonly Guid _instructorProfileId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInput_StartsAsDraftWithTrimmedFields()
    {
        // Arrange / Act
        var course = CourseAggregate.Create(_instructorProfileId, _categoryId, "  Lập trình C# ", "slug", "  Mô tả  ", 499_000m, _now);

        // Assert
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Equal("Lập trình C#", course.Title);
        Assert.Equal("Mô tả", course.Description);
        Assert.Equal(499_000m, course.Price);
        Assert.Equal(_instructorProfileId, course.InstructorProfileId);
        Assert.Equal(_categoryId, course.CategoryId);
        Assert.Equal(_now, course.CreatedAt);
        Assert.Null(course.UpdatedAt);
        Assert.NotEqual(Guid.Empty, course.CourseId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankDescription_StoresNull(string? description)
    {
        // Arrange / Act
        var course = CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", "slug", description, 0m, _now);

        // Assert
        Assert.Null(course.Description);
        Assert.Equal(0m, course.Price);
    }

    [Fact]
    public void Update_ValidInput_ReplacesFieldsKeepsOwnerAndStatusAndSetsUpdatedAt()
    {
        // Arrange
        var course = CourseAggregate.Create(_instructorProfileId, _categoryId, "Cũ", "slug", "Cũ", 1m, _now);
        var newCategoryId = Guid.NewGuid();
        var later = _now.AddHours(1);

        // Act
        course.Update(newCategoryId, "Mới", null, 2m, later);

        // Assert
        Assert.Equal(newCategoryId, course.CategoryId);
        Assert.Equal("Mới", course.Title);
        Assert.Null(course.Description);
        Assert.Equal(2m, course.Price);
        Assert.Equal(_instructorProfileId, course.InstructorProfileId);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Equal(later, course.UpdatedAt);
    }

    [Fact]
    public void Create_EmptyInstructorProfileId_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(Guid.Empty, _categoryId, "Tên", "slug", null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_EmptyCategoryId_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(_instructorProfileId, Guid.Empty, "Tên", "slug", null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_TitleTooLong_ThrowsDomainException()
    {
        // Arrange
        var title = new string('a', CourseAggregate.TitleMaxLength + 1);

        // Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, title, "slug", null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_BlankTitle_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "  ", "slug", null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_DescriptionTooLong_ThrowsDomainException()
    {
        // Arrange
        var description = new string('a', CourseAggregate.DescriptionMaxLength + 1);

        // Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", "slug", description, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10.5)]
    public void Create_NegativeOrFractionalPrice_ThrowsDomainException(double price)
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", "slug", null, (decimal)price, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_PriceAboveColumnLimit_ThrowsDomainException()
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", "slug", null, CourseAggregate.PriceMax + 1, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Update_InvalidInput_KeepsPreviousValues()
    {
        // Arrange
        var course = CourseAggregate.Create(_instructorProfileId, _categoryId, "Giữ", "slug", "Giữ", 5m, _now);

        // Act
        var act = () => course.Update(_categoryId, " ", null, 9m, _now.AddHours(1));

        // Assert
        Assert.Throws<DomainException>(act);
        Assert.Equal("Giữ", course.Title);
        Assert.Equal(5m, course.Price);
        Assert.Null(course.UpdatedAt);
    }

    [Fact]
    public void Update_NewTitle_KeepsSlug()
    {
        // Arrange
        var course = CourseAggregate.Create(_instructorProfileId, _categoryId, "Cũ", "cu", null, 1m, _now);

        // Act
        course.Update(_categoryId, "Hoàn toàn mới", null, 1m, _now.AddHours(1));

        // Assert
        Assert.Equal("cu", course.Slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_BlankSlug_ThrowsDomainException(string slug)
    {
        // Arrange / Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", slug, null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_SlugTooLong_ThrowsDomainException()
    {
        // Arrange
        var slug = new string('a', CourseAggregate.SlugMaxLength + 1);

        // Act
        var act = () => CourseAggregate.Create(_instructorProfileId, _categoryId, "Tên", slug, null, 1m, _now);

        // Assert
        Assert.Equal("INVALID_COURSE", Assert.Throws<DomainException>(act).ErrorCode);
    }
}
