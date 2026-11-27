namespace TeduMarket.Application.Course;

/// <summary>Cổng đọc phía Application; Infrastructure cài đặt bằng Dapper.</summary>
public interface ILessonReader
{
    /// <summary>Mọi Bài học của khóa, xếp theo thứ tự, kèm cờ xem thử.</summary>
    Task<IReadOnlyList<LessonDto>> ListByCourseAsync(Guid courseId, CancellationToken cancellationToken);
}
