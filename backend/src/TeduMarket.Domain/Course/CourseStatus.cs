namespace TeduMarket.Domain.Course;

/// <summary>Trạng thái vòng đời Khóa học; T26 mới có <see cref="Draft"/>, các trạng thái còn lại thêm ở thẻ duyệt khóa.</summary>
public enum CourseStatus
{
    /// <summary>Chưa đăng: khóa mới tạo hoặc chưa gửi duyệt, không hiện công khai.</summary>
    Draft,
}
