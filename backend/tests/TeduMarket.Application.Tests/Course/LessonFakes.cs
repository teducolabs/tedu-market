using TeduMarket.Application.Course;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

internal sealed class FakeLessonRepository(IEnumerable<Lesson>? existing = null) : ILessonRepository
{
    private readonly List<Lesson> _stored = [.. existing ?? []];

    public List<Lesson> Added { get; } = [];

    public List<Lesson> Removed { get; } = [];

    public void Add(Lesson lesson)
    {
        Added.Add(lesson);
        _stored.Add(lesson);
    }

    public void Remove(Lesson lesson) => Removed.Add(lesson);

    public Task<Lesson?> FindAsync(Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult(_stored.SingleOrDefault(lesson => lesson.LessonId == lessonId));

    public Task<IReadOnlyList<Lesson>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Lesson>>([.. _stored.Where(lesson => lesson.CourseId == courseId).OrderBy(lesson => lesson.LessonOrder)]);
}

internal sealed class FakeLessonReader(IReadOnlyList<LessonDto> lessons) : ILessonReader
{
    public Task<IReadOnlyList<LessonDto>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.FromResult(lessons);
}

/// <summary>Khóa có chủ là <see cref="OwnerProfile"/> cùng 3 Bài học xếp thứ tự 1..3.</summary>
internal sealed class LessonScenario
{
    public InstructorProfileDto OwnerProfile { get; } = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);

    public InstructorProfileDto OtherProfile { get; } = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);

    public CourseAggregate Course { get; }

    public List<Lesson> Lessons { get; }

    public FakeCourseRepository Courses { get; }

    public FakeLessonRepository LessonRepository { get; }

    public FakeCategoryUnitOfWork UnitOfWork { get; } = new();

    public DateTimeOffset Now { get; } = new(2026, 11, 27, 0, 0, 0, TimeSpan.Zero);

    public LessonScenario()
    {
        Course = CourseAggregate.Create(OwnerProfile.InstructorProfileId, Guid.NewGuid(), "Khóa", "khoa", null, 1m, Now);
        Lessons = [.. Enumerable.Range(1, 3).Select(order => Lesson.Create(Course.CourseId, $"Bài {order}", order, Now))];
        Courses = new FakeCourseRepository(Course);
        LessonRepository = new FakeLessonRepository(Lessons);
    }

    public FakeCourseProfileReader ProfileReaderFor(bool owner) =>
        new(null, owner ? OwnerProfile : OtherProfile);
}
