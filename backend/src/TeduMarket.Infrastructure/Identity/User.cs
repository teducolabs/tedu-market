using Microsoft.AspNetCore.Identity;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Tài khoản Identity (tên kỹ thuật nội bộ); Học viên, Instructor, Admin phân biệt bằng vai trò.</summary>
public sealed class User : IdentityUser<Guid>
{
}
