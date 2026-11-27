using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Course;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Course;

internal sealed class LessonRepository(AppDbContext dbContext) : ILessonRepository
{
    public void Add(Lesson lesson) => dbContext.Set<Lesson>().Add(lesson);

    public Task<Lesson?> FindAsync(Guid lessonId, CancellationToken cancellationToken) =>
        dbContext.Set<Lesson>().SingleOrDefaultAsync(lesson => lesson.LessonId == lessonId, cancellationToken);

    public void Remove(Lesson lesson) => dbContext.Set<Lesson>().Remove(lesson);

    public async Task<IReadOnlyList<Lesson>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken) =>
        await dbContext.Set<Lesson>()
            .Where(lesson => lesson.CourseId == courseId)
            .OrderBy(lesson => lesson.LessonOrder)
            .ThenBy(lesson => lesson.CreatedAt)
            .ToListAsync(cancellationToken);
}
