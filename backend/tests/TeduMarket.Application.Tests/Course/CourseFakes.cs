using TeduMarket.Application.Course;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

internal sealed class FakeCourseRepository(CourseAggregate? existing = null, bool categoryInUse = false, IReadOnlySet<string>? slugs = null) : ICourseRepository
{
    public List<CourseAggregate> Added { get; } = [];

    public List<CourseAggregate> Removed { get; } = [];

    public string? PrefixSeen { get; private set; }

    public void Add(CourseAggregate course) => Added.Add(course);

    public void Remove(CourseAggregate course) => Removed.Add(course);

    public Task<IReadOnlySet<string>> ListSlugsWithPrefixAsync(string prefix, CancellationToken cancellationToken)
    {
        PrefixSeen = prefix;
        return Task.FromResult(slugs ?? new HashSet<string>());
    }

    public Task<CourseAggregate?> FindAsync(Guid courseId, CancellationToken cancellationToken) => Task.FromResult(existing);

    public Task<bool> ExistsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken) => Task.FromResult(categoryInUse);
}

internal sealed class FakeInstructorCourseGate(bool hasCourses) : IInstructorCourseGate
{
    public Task<bool> HasCoursesAsync(Guid instructorProfileId, CancellationToken cancellationToken) => Task.FromResult(hasCourses);
}

internal sealed class FakeCourseProfileReader(InstructorProfileDto? byId, InstructorProfileDto? byUserId) : IInstructorProfileReader
{
    public Task<InstructorProfileDto?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) => Task.FromResult(byId);

    public Task<InstructorProfileDto?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(byUserId);
}
