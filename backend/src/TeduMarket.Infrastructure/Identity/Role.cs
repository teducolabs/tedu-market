using Microsoft.AspNetCore.Identity;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Vai trò Identity; giá trị tên xem <see cref="Domain.Identity.RoleNames"/>.</summary>
public sealed class Role : IdentityRole<Guid>
{
}
