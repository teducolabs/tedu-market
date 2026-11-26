namespace TeduMarket.Application.Instructor;

/// <param name="InstructorProfileId">Id Hồ sơ Instructor.</param>
/// <param name="UserId">Id tài khoản của Instructor.</param>
/// <param name="DisplayName">Tên hiển thị; <c>null</c> khi chưa cập nhật lần nào.</param>
/// <param name="Biography">Giới thiệu.</param>
/// <param name="AvatarUrl">Url ảnh đại diện.</param>
public sealed record InstructorProfileDto(Guid InstructorProfileId, Guid UserId, string? DisplayName, string? Biography, string? AvatarUrl);
