using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Instructor;
using TeduMarket.Application.Instructor.Queries.GetInstructorProfile;
using TeduMarket.Application.Instructor.Queries.GetMyInstructorProfile;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class GetInstructorProfileQueryHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();

    private InstructorProfileDto Profile => new(Guid.NewGuid(), _ownerId, "A", null, null);

    [Fact]
    public async Task Handle_OwnerInstructor_ReturnsProfileAsync()
    {
        // Arrange
        var profile = Profile;
        var handler = new GetInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Instructor, _ownerId), new FakeProfileReader(profile));

        // Act
        var result = await handler.Handle(new GetInstructorProfileQuery(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        Assert.Equal(profile, result);
    }

    [Fact]
    public async Task Handle_AdminReadsOtherProfile_ReturnsProfileAsync()
    {
        // Arrange
        var profile = Profile;
        var handler = new GetInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Admin), new FakeProfileReader(profile));

        // Act
        var result = await handler.Handle(new GetInstructorProfileQuery(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        Assert.Equal(profile, result);
    }

    [Fact]
    public async Task Handle_InstructorReadsOtherProfile_ThrowsForbiddenAsync()
    {
        // Arrange
        var profile = Profile;
        var handler = new GetInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Instructor, Guid.NewGuid()), new FakeProfileReader(profile));

        // Act
        var act = async () => await handler.Handle(new GetInstructorProfileQuery(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task Handle_Student_ThrowsForbiddenAsync()
    {
        // Arrange
        var profile = Profile;
        var handler = new GetInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Student, _ownerId), new FakeProfileReader(profile));

        // Act
        var act = async () => await handler.Handle(new GetInstructorProfileQuery(profile.InstructorProfileId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new GetInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Admin), new FakeProfileReader(null));

        // Act
        var act = async () => await handler.Handle(new GetInstructorProfileQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task HandleMine_Instructor_ReturnsOwnProfileAsync()
    {
        // Arrange
        var profile = Profile;
        var handler = new GetMyInstructorProfileQueryHandler(
            new FakeCurrentUser(RoleNames.Instructor, _ownerId), new FakeProfileReader(null, profile));

        // Act
        var result = await handler.Handle(new GetMyInstructorProfileQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(profile, result);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Admin)]
    public async Task HandleMine_NotInstructor_ThrowsForbiddenAsync(string role)
    {
        // Arrange
        var handler = new GetMyInstructorProfileQueryHandler(new FakeCurrentUser(role, _ownerId), new FakeProfileReader(null, Profile));

        // Act
        var act = async () => await handler.Handle(new GetMyInstructorProfileQuery(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task HandleMine_NoProfile_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new GetMyInstructorProfileQueryHandler(new FakeCurrentUser(RoleNames.Instructor, _ownerId), new FakeProfileReader(null));

        // Act
        var act = async () => await handler.Handle(new GetMyInstructorProfileQuery(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
