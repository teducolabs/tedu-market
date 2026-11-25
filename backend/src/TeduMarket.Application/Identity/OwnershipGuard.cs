using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Identity;

/// <summary>Mẫu kiểm tra sở hữu trong Handler: chủ sở hữu hoặc Admin mới được đi tiếp, ngược lại <see cref="ForbiddenException"/> (403).</summary>
public static class OwnershipGuard
{
    public static void EnsureOwnerOrAdmin(ICurrentUser currentUser, Guid ownerId)
    {
        if (currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (currentUser.UserId != ownerId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập tài nguyên này");
        }
    }
}
