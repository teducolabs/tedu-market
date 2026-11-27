namespace TeduMarket.Application.Course;

/// <param name="LessonId">Id Bài học.</param>
/// <param name="CourseId">Id Khóa học chứa bài.</param>
/// <param name="LessonTitle">Tiêu đề bài học.</param>
/// <param name="LessonOrder">Thứ tự trong khóa, từ 1.</param>
/// <param name="IsPreview">Có phải Bài học xem thử hay không.</param>
public sealed record LessonDto(Guid LessonId, Guid CourseId, string LessonTitle, int LessonOrder, bool IsPreview);
