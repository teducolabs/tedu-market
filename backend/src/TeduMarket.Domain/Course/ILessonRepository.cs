namespace TeduMarket.Domain.Course;

public interface ILessonRepository
{
    void Add(Lesson lesson);

    Task<Lesson?> FindAsync(Guid lessonId, CancellationToken cancellationToken);

    void Remove(Lesson lesson);

    /// <summary>Mọi Bài học của khóa, xếp theo <see cref="Lesson.LessonOrder"/>, đang được theo dõi để sửa thứ tự.</summary>
    Task<IReadOnlyList<Lesson>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken);
}
