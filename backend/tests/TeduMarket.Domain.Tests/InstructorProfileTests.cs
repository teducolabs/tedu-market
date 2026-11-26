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
}
