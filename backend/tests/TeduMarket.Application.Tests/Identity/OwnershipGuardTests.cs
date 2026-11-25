using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Identity;

[Trait("Category", "Unit")]
public sealed class OwnershipGuardTests
{
    [Fact]
    public void EnsureOwnerOrAdmin_UserIsOwner_DoesNotThrow()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser(ownerId, RoleNames.Student);

        // Act
        var exception = Record.Exception(() => OwnershipGuard.EnsureOwnerOrAdmin(currentUser, ownerId));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureOwnerOrAdmin_UserIsNotOwner_ThrowsForbidden()
    {
        // Arrange
        var currentUser = new FakeCurrentUser(Guid.NewGuid(), RoleNames.Instructor);

        // Act
        var exception = Record.Exception(() => OwnershipGuard.EnsureOwnerOrAdmin(currentUser, Guid.NewGuid()));

        // Assert
        Assert.IsType<ForbiddenException>(exception);
    }

    [Fact]
    public void EnsureOwnerOrAdmin_UserIsAdminButNotOwner_DoesNotThrow()
    {
        // Arrange
        var currentUser = new FakeCurrentUser(Guid.NewGuid(), RoleNames.Admin);

        // Act
        var exception = Record.Exception(() => OwnershipGuard.EnsureOwnerOrAdmin(currentUser, Guid.NewGuid()));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureOwnerOrAdmin_Guest_ThrowsForbidden()
    {
        // Arrange
        var currentUser = new FakeCurrentUser(null);

        // Act
        var exception = Record.Exception(() => OwnershipGuard.EnsureOwnerOrAdmin(currentUser, Guid.NewGuid()));

        // Assert
        Assert.IsType<ForbiddenException>(exception);
    }

    private sealed class FakeCurrentUser(Guid? userId, params string[] roles) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public bool IsInRole(string roleName) => roles.Contains(roleName);
    }
}
