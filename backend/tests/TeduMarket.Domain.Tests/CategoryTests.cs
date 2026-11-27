using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class CategoryTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 27, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidName_TrimsNameAndSetsCreatedAt()
    {
        // Arrange
        var name = "  Lập trình web ";

        // Act
        var category = Category.Create(name, _now);

        // Assert
        Assert.Equal("Lập trình web", category.Name);
        Assert.Equal(_now, category.CreatedAt);
        Assert.Null(category.UpdatedAt);
        Assert.NotEqual(Guid.Empty, category.CategoryId);
    }

    [Fact]
    public void ToNormalizedName_NamesDifferingOnlyByCaseAndSpaces_ReturnSameKey()
    {
        // Arrange / Act
        var lower = Category.ToNormalizedName("lập trình");
        var upper = Category.ToNormalizedName("  LẬP TRÌNH ");

        // Assert
        Assert.Equal(lower, upper);
    }

    [Fact]
    public void Rename_ValidName_UpdatesNormalizedName()
    {
        // Arrange
        var category = Category.Create("Cũ", _now);

        // Act
        category.Rename("Mới", _now);

        // Assert
        Assert.Equal("MỚI", category.NormalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_ThrowsDomainException(string? name)
    {
        // Arrange / Act
        var act = () => Category.Create(name, _now);

        // Assert
        Assert.Equal("INVALID_CATEGORY", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Create_NameLongerThanLimit_ThrowsDomainException()
    {
        // Arrange
        var name = new string('a', Category.NameMaxLength + 1);

        // Act
        var act = () => Category.Create(name, _now);

        // Assert
        Assert.Equal("INVALID_CATEGORY", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void Rename_ValidName_ChangesNameAndSetsUpdatedAt()
    {
        // Arrange
        var category = Category.Create("Cũ", _now);
        var later = _now.AddHours(1);

        // Act
        category.Rename(" Mới ", later);

        // Assert
        Assert.Equal("Mới", category.Name);
        Assert.Equal(later, category.UpdatedAt);
    }

    [Fact]
    public void Rename_BlankName_ThrowsDomainExceptionAndKeepsName()
    {
        // Arrange
        var category = Category.Create("Giữ nguyên", _now);

        // Act
        var act = () => category.Rename(" ", _now);

        // Assert
        Assert.Equal("INVALID_CATEGORY", Assert.Throws<DomainException>(act).ErrorCode);
        Assert.Equal("Giữ nguyên", category.Name);
    }
}
