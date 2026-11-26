using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor.Commands.ApproveInstructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Tests.Instructor;

[Trait("Category", "Unit")]
public sealed class ApproveInstructorCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly FakeUserRoleService _roles = new();
    private readonly FakeRepository _repository = new();
    private readonly FakeSessionRevoker _revoker = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 26, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_StudentApprovedByAdmin_StagesRoleAddsProfileAndSavesOnceAsync()
    {
        // Arrange
        _roles.Account = new UserAccount(_userId, "hocvien@example.test", [RoleNames.Student]);
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var result = await handler.Handle(new ApproveInstructorCommand(_userId), CancellationToken.None);

        // Assert
        Assert.Equal([(_userId, RoleNames.Instructor)], _roles.Staged);
        var profile = Assert.Single(_repository.Added);
        Assert.Equal(_userId, profile.UserId);
        Assert.Equal(_clock.GetUtcNow(), profile.CreatedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
        Assert.Equal([_userId], _revoker.Revoked);
        Assert.Equal(new InstructorApprovalDto(_userId, profile.InstructorProfileId), result);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    public async Task Handle_NotAdmin_ThrowsForbiddenWithoutChangesAsync(string role)
    {
        // Arrange
        _roles.Account = new UserAccount(_userId, null, [RoleNames.Student]);
        var handler = CreateHandler(role);

        // Act
        var act = async () => await handler.Handle(new ApproveInstructorCommand(_userId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        AssertNothingChanged();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundWithoutChangesAsync()
    {
        // Arrange
        _roles.Account = null;
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new ApproveInstructorCommand(_userId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        AssertNothingChanged();
    }

    [Fact]
    public async Task Handle_AlreadyInstructor_ThrowsDomainExceptionWithoutChangesAsync()
    {
        // Arrange
        _roles.Account = new UserAccount(_userId, null, [RoleNames.Student, RoleNames.Instructor]);
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new ApproveInstructorCommand(_userId), CancellationToken.None);

        // Assert
        Assert.Equal("INSTRUCTOR_ALREADY_APPROVED", (await Assert.ThrowsAsync<DomainException>(act)).ErrorCode);
        AssertNothingChanged();
    }

    [Fact]
    public async Task Handle_UserWithoutStudentRole_ThrowsDomainExceptionWithoutChangesAsync()
    {
        // Arrange
        _roles.Account = new UserAccount(_userId, null, [RoleNames.Admin]);
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new ApproveInstructorCommand(_userId), CancellationToken.None);

        // Assert
        Assert.Equal("USER_NOT_STUDENT", (await Assert.ThrowsAsync<DomainException>(act)).ErrorCode);
        AssertNothingChanged();
    }

    private void AssertNothingChanged()
    {
        Assert.Empty(_roles.Staged);
        Assert.Empty(_repository.Added);
        Assert.Empty(_revoker.Revoked);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private ApproveInstructorCommandHandler CreateHandler(string callerRole) =>
        new(new FakeCurrentUser(callerRole), _roles, _revoker, _repository, _unitOfWork, _clock);

    private sealed class FakeCurrentUser(string role) : ICurrentUser
    {
        public Guid? UserId { get; } = Guid.NewGuid();

        public bool IsInRole(string roleName) => roleName == role;
    }

    private sealed class FakeUserRoleService : IUserRoleService
    {
        public UserAccount? Account { get; set; }

        public List<(Guid UserId, string RoleName)> Staged { get; } = [];

        public Task<UserAccount?> FindAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(Account);

        public Task StageRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
        {
            Staged.Add((userId, roleName));
            return Task.CompletedTask;
        }

        public Task StageRoleRemovalAsync(Guid userId, string roleName, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakeRepository : IInstructorProfileRepository
    {
        public List<InstructorProfile> Added { get; } = [];

        public void Add(InstructorProfile instructorProfile) => Added.Add(instructorProfile);

        public Task<InstructorProfile?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public void Remove(InstructorProfile instructorProfile) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
