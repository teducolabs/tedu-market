using TeduMarket.Domain.Common;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Domain.Tests;

[Trait("Category", "Unit")]
public sealed class InstructorProfileTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 26, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidUserId_ReturnsProfileOfThatUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var instructorProfile = InstructorProfile.Create(userId, _now);

        // Assert
        Assert.Equal(userId, instructorProfile.UserId);
        Assert.Equal(_now, instructorProfile.CreatedAt);
        Assert.NotEqual(Guid.Empty, instructorProfile.InstructorProfileId);
    }

    [Fact]
    public void Create_EmptyUserId_ThrowsDomainException()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var act = () => InstructorProfile.Create(userId, _now);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_USER", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void UpdateContent_ValidValues_TrimsAndStoresThemWithUpdatedAt()
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);
        var updatedAt = _now.AddHours(1);

        // Act
        instructorProfile.UpdateContent("  Nguyễn Văn A ", " Giảng dạy 10 năm ", " https://cdn.example.test/a.png ", updatedAt);

        // Assert
        Assert.Equal("Nguyễn Văn A", instructorProfile.DisplayName);
        Assert.Equal("Giảng dạy 10 năm", instructorProfile.Biography);
        Assert.Equal("https://cdn.example.test/a.png", instructorProfile.AvatarUrl);
        Assert.Equal(updatedAt, instructorProfile.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_BlankOptionalFields_StoresNull()
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);
        instructorProfile.UpdateContent("A", "Giới thiệu", "https://cdn.example.test/a.png", _now);

        // Act
        instructorProfile.UpdateContent("A", "   ", "", _now);

        // Assert
        Assert.Null(instructorProfile.Biography);
        Assert.Null(instructorProfile.AvatarUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateContent_BlankDisplayName_ThrowsDomainExceptionAndKeepsProfile(string? displayName)
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);

        // Act
        var act = () => instructorProfile.UpdateContent(displayName, null, null, _now);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_PROFILE", Assert.Throws<DomainException>(act).ErrorCode);
        Assert.Null(instructorProfile.DisplayName);
        Assert.Null(instructorProfile.UpdatedAt);
    }

    [Fact]
    public void UpdateContent_DisplayNameTooLong_ThrowsDomainException()
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);
        var displayName = new string('a', InstructorProfile.DisplayNameMaxLength + 1);

        // Act
        var act = () => instructorProfile.UpdateContent(displayName, null, null, _now);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_PROFILE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Fact]
    public void UpdateContent_BiographyTooLong_ThrowsDomainException()
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);
        var biography = new string('a', InstructorProfile.BiographyMaxLength + 1);

        // Act
        var act = () => instructorProfile.UpdateContent("A", biography, null, _now);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_PROFILE", Assert.Throws<DomainException>(act).ErrorCode);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.test/a.png")]
    [InlineData("not a url")]
    public void UpdateContent_AvatarUrlNotHttp_ThrowsDomainException(string avatarUrl)
    {
        // Arrange
        var instructorProfile = InstructorProfile.Create(Guid.NewGuid(), _now);

        // Act
        var act = () => instructorProfile.UpdateContent("A", null, avatarUrl, _now);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_PROFILE", Assert.Throws<DomainException>(act).ErrorCode);
    }
}
