using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Tests.Course;
using TeduMarket.Domain.Common;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class DeleteInstructorProfileCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 26, 0, 0, 0, TimeSpan.Zero);

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeProfileUnitOfWork _unitOfWork = new();
    private readonly FakeSessionRevoker _revoker = new();

    [Fact]
    public async Task Handle_AdminDeletesProfile_RemovesProfileAndInstructorRoleInOneSaveAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _now);
        var repository = new FakeProfileRepository(profile);
        var roles = new FakeProfileRoleService(new UserAccount(_ownerId, null, [RoleNames.Student, RoleNames.Instructor]));
        var handler = new DeleteInstructorProfileCommandHandler(new FakeCurrentUser(RoleNames.Admin), roles, _revoker, repository, new FakeInstructorCourseGate(false), _unitOfWork);

        // Act
        await handler.Handle(new DeleteInstructorProfileCommand(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        Assert.Equal([profile], repository.Removed);
        Assert.Equal([(_ownerId, RoleNames.Instructor)], roles.Removed);
        Assert.Equal(1, _unitOfWork.SaveCount);
        Assert.Equal([_ownerId], _revoker.Revoked);
    }

    [Fact]
    public async Task Handle_OwnerHasNoInstructorRole_RemovesOnlyProfileAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _now);
        var repository = new FakeProfileRepository(profile);
        var roles = new FakeProfileRoleService(new UserAccount(_ownerId, null, [RoleNames.Student]));
        var handler = new DeleteInstructorProfileCommandHandler(new FakeCurrentUser(RoleNames.Admin), roles, _revoker, repository, new FakeInstructorCourseGate(false), _unitOfWork);

        // Act
        await handler.Handle(new DeleteInstructorProfileCommand(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        Assert.Equal([profile], repository.Removed);
        Assert.Empty(roles.Removed);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(RoleNames.Instructor)]
    [InlineData(RoleNames.Student)]
    public async Task Handle_NotAdmin_ThrowsForbiddenWithoutChangesAsync(string role)
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _now);
        var repository = new FakeProfileRepository(profile);
        var roles = new FakeProfileRoleService(new UserAccount(_ownerId, null, [RoleNames.Instructor]));
        var handler = new DeleteInstructorProfileCommandHandler(new FakeCurrentUser(role, _ownerId), roles, _revoker, repository, new FakeInstructorCourseGate(false), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteInstructorProfileCommand(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(repository.Removed);
        Assert.Empty(roles.Removed);
        Assert.Equal(0, _unitOfWork.SaveCount);
        Assert.Empty(_revoker.Revoked);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsNotFoundAsync()
    {
        // Arrange
        var repository = new FakeProfileRepository(null);
        var roles = new FakeProfileRoleService(null);
        var handler = new DeleteInstructorProfileCommandHandler(new FakeCurrentUser(RoleNames.Admin), roles, _revoker, repository, new FakeInstructorCourseGate(false), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteInstructorProfileCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorStillHasCourses_ThrowsInstructorHasCoursesAndKeepsProfileAndRoleAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _now);
        var repository = new FakeProfileRepository(profile);
        var roles = new FakeProfileRoleService(new UserAccount(_ownerId, null, [RoleNames.Student, RoleNames.Instructor]));
        var handler = new DeleteInstructorProfileCommandHandler(
            new FakeCurrentUser(RoleNames.Admin), roles, _revoker, repository, new FakeInstructorCourseGate(true), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteInstructorProfileCommand(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<DomainException>(act);
        Assert.Equal("INSTRUCTOR_HAS_COURSES", exception.ErrorCode);
        Assert.Empty(repository.Removed);
        Assert.Empty(roles.Removed);
        Assert.Equal(0, _unitOfWork.SaveCount);
        Assert.Empty(_revoker.Revoked);
    }
}
