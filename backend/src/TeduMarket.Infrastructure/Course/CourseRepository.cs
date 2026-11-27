using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Course;
using TeduMarket.Infrastructure.Persistence;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Infrastructure.Course;

internal sealed class CourseRepository(AppDbContext dbContext) : ICourseRepository
{
    public void Add(CourseAggregate course) => dbContext.Set<CourseAggregate>().Add(course);

    public Task<CourseAggregate?> FindAsync(Guid courseId, CancellationToken cancellationToken) =>
        dbContext.Set<CourseAggregate>().SingleOrDefaultAsync(course => course.CourseId == courseId, cancellationToken);

    public void Remove(CourseAggregate course) => dbContext.Set<CourseAggregate>().Remove(course);

    public async Task<IReadOnlySet<string>> ListSlugsWithPrefixAsync(string prefix, CancellationToken cancellationToken) =>
        (await dbContext.Set<CourseAggregate>()
            .Where(course => course.Slug.StartsWith(prefix))
            .Select(course => course.Slug)
            .ToListAsync(cancellationToken)).ToHashSet();

    public Task<bool> ExistsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Set<CourseAggregate>().AnyAsync(course => course.CategoryId == categoryId, cancellationToken);
}
