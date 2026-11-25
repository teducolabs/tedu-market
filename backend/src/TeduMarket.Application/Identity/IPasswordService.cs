namespace TeduMarket.Application.Identity;

/// <summary>Đổi mật khẩu của tài khoản có mật khẩu (Admin, FR-42); cài đặt ở Infrastructure bằng ASP.NET Core Identity.</summary>
public interface IPasswordService
{
    Task<PasswordChangeResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}

public enum PasswordChangeOutcome
{
    Succeeded,
    UserNotFound,
    WrongCurrentPassword,
    NewPasswordRejected,
}

/// <param name="Outcome">Kết quả đổi mật khẩu.</param>
/// <param name="NewPasswordErrors">Thông điệp tiếng Việt khi <see cref="PasswordChangeOutcome.NewPasswordRejected"/>.</param>
public sealed record PasswordChangeResult(PasswordChangeOutcome Outcome, IReadOnlyList<string> NewPasswordErrors)
{
    public static PasswordChangeResult Of(PasswordChangeOutcome outcome) => new(outcome, []);
}
