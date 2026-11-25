namespace TeduMarket.Application.Identity;

/// <summary>Người dùng của request hiện tại; Handler dùng để kiểm tra sở hữu (NFR-08).</summary>
public interface ICurrentUser
{
    /// <summary>Id người dùng đã đăng nhập; <c>null</c> khi Khách.</summary>
    Guid? UserId { get; }

    bool IsInRole(string roleName);
}
