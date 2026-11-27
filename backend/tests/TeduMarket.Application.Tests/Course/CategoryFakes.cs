using TeduMarket.Application.Course;
using TeduMarket.Application.Identity;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Tests.Course;

internal sealed class FakeCategoryCurrentUser(string role) : ICurrentUser
{
    public Guid? UserId { get; } = Guid.NewGuid();

    public bool IsInRole(string roleName) => roleName == role;
}

internal sealed class FakeCategoryRepository(Category? existing = null, bool nameTaken = false) : ICategoryRepository
{
    public List<Category> Added { get; } = [];

    public List<Category> Removed { get; } = [];

    public Guid? ExcludedIdSeen { get; private set; }

    public void Add(Category category) => Added.Add(category);

    public Task<Category?> FindAsync(Guid categoryId, CancellationToken cancellationToken) => Task.FromResult(existing);

    public Task<bool> ExistsWithNameAsync(string name, Guid? excludedCategoryId, CancellationToken cancellationToken)
    {
        ExcludedIdSeen = excludedCategoryId;
        return Task.FromResult(nameTaken);
    }

    public void Remove(Category category) => Removed.Add(category);
}

internal sealed class FakeCategoryUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeCategoryReader(IReadOnlyList<CategoryDto> categories) : ICategoryReader
{
    public Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken) => Task.FromResult(categories);
}
