using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class UpdateInstructorProfileCommandHandlerTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly FakeProfileUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 26, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_OwnerInstructor_UpdatesOwnProfileAndSavesOnceAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _clock.GetUtcNow());
        var handler = CreateHandler(profile, RoleNames.Instructor, _ownerId);

        // Act
        var result = await handler.Handle(
            new UpdateInstructorProfileCommand(profile.InstructorProfileId, "Nguyễn Văn A", "Giới thiệu", "https://cdn.example.test/a.png"),
            CancellationToken.None);

        // Assert
        Assert.Equal("Nguyễn Văn A", profile.DisplayName);
        Assert.Equal(_clock.GetUtcNow(), profile.UpdatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
        Assert.Equal(
            new TeduMarket.Application.Instructor.InstructorProfileDto(
                profile.InstructorProfileId, _ownerId, "Nguyễn Văn A", "Giới thiệu", "https://cdn.example.test/a.png"),
            result);
    }

    [Fact]
    public async Task Handle_AdminEditsOtherInstructorProfile_UpdatesAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _clock.GetUtcNow());
        var handler = CreateHandler(profile, RoleNames.Admin, Guid.NewGuid());

        // Act
        await handler.Handle(new UpdateInstructorProfileCommand(profile.InstructorProfileId, "Tên do Admin sửa", null, null), CancellationToken.None);

        // Assert
        Assert.Equal("Tên do Admin sửa", profile.DisplayName);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorEditsOtherInstructorProfile_ThrowsForbiddenWithoutChangesAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _clock.GetUtcNow());
        var handler = CreateHandler(profile, RoleNames.Instructor, Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(
            new UpdateInstructorProfileCommand(profile.InstructorProfileId, "Chiếm hồ sơ", null, null), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Null(profile.DisplayName);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_Student_ThrowsForbiddenWithoutChangesAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _clock.GetUtcNow());
        var handler = CreateHandler(profile, RoleNames.Student, _ownerId);

        // Act
        var act = async () => await handler.Handle(
            new UpdateInstructorProfileCommand(profile.InstructorProfileId, "A", null, null), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = CreateHandler(null, RoleNames.Admin, Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(
            new UpdateInstructorProfileCommand(Guid.NewGuid(), "A", null, null), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InvalidContent_ThrowsDomainExceptionWithoutSavingAsync()
    {
        // Arrange
        var profile = InstructorProfile.Create(_ownerId, _clock.GetUtcNow());
        var handler = CreateHandler(profile, RoleNames.Instructor, _ownerId);

        // Act
        var act = async () => await handler.Handle(
            new UpdateInstructorProfileCommand(profile.InstructorProfileId, " ", null, null), CancellationToken.None);

        // Assert
        Assert.Equal("INVALID_INSTRUCTOR_PROFILE", (await Assert.ThrowsAsync<DomainException>(act)).ErrorCode);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private UpdateInstructorProfileCommandHandler CreateHandler(InstructorProfile? profile, string callerRole, Guid callerId) =>
        new(new FakeCurrentUser(callerRole, callerId), new FakeProfileRepository(profile), _unitOfWork, _clock);
}
