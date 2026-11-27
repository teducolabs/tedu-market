namespace TeduMarket.Application.Course;

/// <summary>Cổng của module Course cho module Instructor hỏi về khóa học của một Instructor mà không đụng bảng Course (6.3).</summary>
public interface IInstructorCourseGate
{
    /// <summary>Instructor còn Khóa học nào (mọi trạng thái, kể cả đã có Enrollment) không.</summary>
    Task<bool> HasCoursesAsync(Guid instructorProfileId, CancellationToken cancellationToken);
}
