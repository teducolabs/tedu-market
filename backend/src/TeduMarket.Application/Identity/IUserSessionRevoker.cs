namespace TeduMarket.Application.Identity;

/// <summary>Cổng của Identity cho module khác: thu hồi mọi token của một người dùng để lần đăng nhập sau nhận đúng vai trò. Cài đặt ở Infrastructure.</summary>
public interface IUserSessionRevoker
{
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);
}
