namespace TeduMarket.Application.Identity.Queries.ListUsers;

/// <param name="UserId">Id tài khoản.</param>
/// <param name="Email">Email đăng nhập.</param>
/// <param name="Roles">Tên các vai trò.</param>
/// <param name="InstructorProfileId">Id Hồ sơ Instructor; <c>null</c> nếu chưa được duyệt.</param>
public sealed record UserDto(Guid UserId, string? Email, IReadOnlyList<string> Roles, Guid? InstructorProfileId);
