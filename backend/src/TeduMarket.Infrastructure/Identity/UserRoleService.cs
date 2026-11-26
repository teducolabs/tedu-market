using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeduMarket.Application.Identity;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Đọc vai trò và gán vai trò qua <see cref="AppDbContext"/> dùng chung để lần lưu của Handler chốt cả vai trò lẫn dữ liệu module khác.</summary>
internal sealed class UserRoleService(AppDbContext dbContext) : IUserRoleService
{
    public async Task<UserAccount?> FindAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking()
            .Where(candidate => candidate.Id == userId)
            .Select(candidate => new { candidate.Id, candidate.Email })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await (
            from userRole in dbContext.UserRoles.AsNoTracking()
            join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name!).ToListAsync(cancellationToken);
        return new UserAccount(user.Id, user.Email, roles);
    }

    public async Task StageRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var roleId = await dbContext.Roles.AsNoTracking()
            .Where(role => role.Name == roleName)
            .Select(role => role.Id)
            .SingleAsync(cancellationToken);
        dbContext.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });
    }
}
