namespace TeduMarket.Application.Notification;

/// <summary>Liên kết công khai tới Web học viên, dùng trong nội dung email.</summary>
public interface IStudentWebLinks
{
    /// <summary>Địa chỉ trang Danh sách khóa học (<c>/courses</c>).</summary>
    Uri CourseListUrl { get; }
}
