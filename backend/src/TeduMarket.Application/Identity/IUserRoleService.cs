namespace TeduMarket.Application.Identity;

/// <summary>Cổng của Identity cho module khác: đọc tài khoản và gán vai trò. Cài đặt ở Infrastructure.</summary>
public interface IUserRoleService
{
    Task<UserAccount?> FindAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Đánh dấu gán vai trò nhưng chưa lưu: lưu cùng lần <c>IUnitOfWork.SaveChangesAsync</c> của Handler để cùng transaction.</summary>
    Task StageRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
}

/// <param name="UserId">Id tài khoản.</param>
/// <param name="Email">Email đăng nhập.</param>
/// <param name="Roles">Tên các vai trò đang có.</param>
public sealed record UserAccount(Guid UserId, string? Email, IReadOnlyList<string> Roles);
