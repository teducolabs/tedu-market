using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Course;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Course;

internal sealed class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public void Add(Category category) => dbContext.Set<Category>().Add(category);

    public Task<Category?> FindAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Set<Category>().SingleOrDefaultAsync(category => category.CategoryId == categoryId, cancellationToken);

    public Task<bool> ExistsWithNameAsync(string name, Guid? excludedCategoryId, CancellationToken cancellationToken)
    {
        var normalizedName = Category.ToNormalizedName(name);
        return dbContext.Set<Category>().AnyAsync(
            category => category.NormalizedName == normalizedName && category.CategoryId != excludedCategoryId, cancellationToken);
    }

    public void Remove(Category category) => dbContext.Set<Category>().Remove(category);
}
