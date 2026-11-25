using Microsoft.AspNetCore.Identity;
using TeduMarket.Application.Identity;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Đổi mật khẩu bằng <see cref="UserManager{TUser}"/> (lưu dạng băm, làm mới security stamp) rồi thu hồi mọi token hiện có của người dùng.</summary>
public sealed class PasswordService(UserManager<User> userManager, UserSessionRevoker sessionRevoker) : IPasswordService
{
    public async Task<PasswordChangeResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return PasswordChangeResult.Of(PasswordChangeOutcome.UserNotFound);
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await sessionRevoker.RevokeAllAsync(userId, cancellationToken);
            return PasswordChangeResult.Of(PasswordChangeOutcome.Succeeded);
        }

        if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.PasswordMismatch)))
        {
            return PasswordChangeResult.Of(PasswordChangeOutcome.WrongCurrentPassword);
        }

        return new PasswordChangeResult(PasswordChangeOutcome.NewPasswordRejected, [.. result.Errors.Select(error => Translate(error.Code))]);
    }

    private static string Translate(string code) => code switch
    {
        nameof(IdentityErrorDescriber.PasswordTooShort) => "Mật khẩu cần tối thiểu 6 ký tự",
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) => "Mật khẩu cần có ít nhất 1 chữ số",
        nameof(IdentityErrorDescriber.PasswordRequiresLower) => "Mật khẩu cần có ít nhất 1 chữ thường",
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) => "Mật khẩu cần có ít nhất 1 chữ hoa",
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => "Mật khẩu cần có ít nhất 1 ký tự đặc biệt",
        _ => "Mật khẩu mới chưa đủ mạnh",
    };
}
