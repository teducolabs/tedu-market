namespace TeduMarket.Domain.Identity;

/// <summary>Tên 3 vai trò hệ thống. Instructor giữ thêm vai trò Student (Duyệt Instructor chỉ thêm vai trò, không thay thế).</summary>
public static class RoleNames
{
    public const string Student = "Student";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";
}
