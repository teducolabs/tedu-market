using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Tests.Instructor;

internal sealed class FakeCurrentUser(string role, Guid? userId = null) : ICurrentUser
{
    public Guid? UserId { get; } = userId ?? Guid.NewGuid();

    public bool IsInRole(string roleName) => roleName == role;
}

internal sealed class FakeProfileRepository(InstructorProfile? existing) : IInstructorProfileRepository
{
    public List<InstructorProfile> Removed { get; } = [];

    public void Add(InstructorProfile instructorProfile) => throw new NotSupportedException();

    public Task<InstructorProfile?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) =>
        Task.FromResult(existing);

    public void Remove(InstructorProfile instructorProfile) => Removed.Add(instructorProfile);
}

internal sealed class FakeProfileUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeProfileRoleService(UserAccount? account) : IUserRoleService
{
    public List<(Guid UserId, string RoleName)> Removed { get; } = [];

    public Task<UserAccount?> FindAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(account);

    public Task StageRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task StageRoleRemovalAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        Removed.Add((userId, roleName));
        return Task.CompletedTask;
    }
}

internal sealed class FakeSessionRevoker : IUserSessionRevoker
{
    public List<Guid> Revoked { get; } = [];

    public Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        Revoked.Add(userId);
        return Task.CompletedTask;
    }
}

internal sealed class FakeProfileReader(InstructorProfileDto? byId, InstructorProfileDto? byUserId = null) : IInstructorProfileReader
{
    public Task<InstructorProfileDto?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) => Task.FromResult(byId);

    public Task<InstructorProfileDto?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(byUserId);
}
