using TeduMarket.Domain.Course;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class CourseSlugTests
{
    [Theory]
    [InlineData("Lập trình C# cơ bản", "lap-trinh-c-co-ban")]
    [InlineData("  Đồ họa   Đẹp!! ", "do-hoa-dep")]
    [InlineData("ASP.NET Core 10", "asp-net-core-10")]
    public void From_Title_ReturnsLowercaseAsciiWithDashes(string title, string expected)
    {
        // Arrange / Act
        var slug = CourseSlug.From(title);

        // Assert
        Assert.Equal(expected, slug);
    }

    [Theory]
    [InlineData("###")]
    [InlineData("   ")]
    public void From_NoAlphanumericCharacters_ReturnsFallback(string title)
    {
        // Arrange / Act
        var slug = CourseSlug.From(title);

        // Assert
        Assert.Equal(CourseSlug.Fallback, slug);
    }

    [Fact]
    public void From_VeryLongTitle_CutsToBaseMaxLengthWithoutTrailingDash()
    {
        // Arrange
        var title = string.Join(' ', Enumerable.Repeat("abcd", 40));

        // Act
        var slug = CourseSlug.From(title);

        // Assert
        Assert.True(slug.Length <= CourseSlug.BaseMaxLength);
        Assert.False(slug.EndsWith('-'));
    }

    [Fact]
    public void MakeUnique_BaseNotTaken_ReturnsBase()
    {
        // Arrange / Act
        var slug = CourseSlug.MakeUnique("lap-trinh", new HashSet<string> { "lap-trinh-2" });

        // Assert
        Assert.Equal("lap-trinh", slug);
    }

    [Fact]
    public void MakeUnique_BaseAndFirstSuffixTaken_ReturnsNextSuffix()
    {
        // Arrange / Act
        var slug = CourseSlug.MakeUnique("lap-trinh", new HashSet<string> { "lap-trinh", "lap-trinh-2" });

        // Assert
        Assert.Equal("lap-trinh-3", slug);
    }
}
