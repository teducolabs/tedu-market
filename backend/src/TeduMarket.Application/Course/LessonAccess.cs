using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course;

internal static class LessonAccess
{
    /// <summary>Tìm khóa rồi kiểm tra Admin/chủ khóa ở Handler (NFR-08); không có khóa → 404, không phải chủ → 403.</summary>
    public static async Task<Domain.Course.Course> FindCourseForModifyAsync(
        ICurrentUser currentUser,
        IInstructorProfileReader profileReader,
        ICourseRepository courseRepository,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var course = await courseRepository.FindAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Khóa học");
        await CourseAccess.EnsureCanModifyAsync(currentUser, profileReader, course, cancellationToken);
        return course;
    }

    /// <summary>Bài học phải thuộc đúng khóa trên đường dẫn, nếu không coi như không tồn tại.</summary>
    public static async Task<Lesson> FindLessonOfCourseAsync(
        ILessonRepository lessonRepository, Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        var lesson = await lessonRepository.FindAsync(lessonId, cancellationToken);
        return lesson is not null && lesson.CourseId == courseId
            ? lesson
            : throw new NotFoundException("Không tìm thấy Bài học");
    }

    public static LessonDto ToDto(Lesson lesson) =>
        new(lesson.LessonId, lesson.CourseId, lesson.LessonTitle, lesson.LessonOrder, lesson.IsPreview);
}
