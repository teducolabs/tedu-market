using Microsoft.AspNetCore.Identity;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Kết quả tìm hoặc tạo tài khoản cho một lần đăng nhập Google.</summary>
public enum GoogleAccountOutcome
{
    /// <summary>Tài khoản đã có, đăng nhập lại.</summary>
    ExistingAccount,

    /// <summary>Lần đầu: vừa tạo tài khoản Học viên.</summary>
    StudentCreated,

    /// <summary>Google không trả email đã xác minh.</summary>
    EmailNotVerified,

    /// <summary>Email đã thuộc tài khoản khác chưa liên kết Google (ví dụ Admin đăng nhập bằng mật khẩu).</summary>
    EmailInUse,

    /// <summary>Không tạo được tài khoản (lỗi Identity).</summary>
    Failed,
}

public sealed record GoogleAccountResult(GoogleAccountOutcome Outcome, User? User)
{
    public bool Succeeded => Outcome is GoogleAccountOutcome.ExistingAccount or GoogleAccountOutcome.StudentCreated;
}

/// <summary>
/// Tìm tài khoản theo định danh Google; lần đầu tạo tài khoản Học viên (FR-03). Tạo tài khoản, gán vai trò và liên kết Google
/// nằm trong một transaction để không để lại tài khoản thiếu vai trò.
/// </summary>
public sealed class GoogleAccountService(UserManager<User> userManager, AppDbContext dbContext)
{
    public async Task<GoogleAccountResult> FindOrCreateStudentAsync(
        string loginProvider,
        string providerKey,
        string? email,
        bool emailVerified,
        CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByLoginAsync(loginProvider, providerKey);
        if (existing is not null)
        {
            return new GoogleAccountResult(GoogleAccountOutcome.ExistingAccount, existing);
        }

        if (string.IsNullOrWhiteSpace(email) || !emailVerified)
        {
            return new GoogleAccountResult(GoogleAccountOutcome.EmailNotVerified, null);
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return new GoogleAccountResult(GoogleAccountOutcome.EmailInUse, null);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var user = new User { UserName = email, Email = email, EmailConfirmed = true };
        var created = await userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            // Hai lần đăng nhập đầu chạy song song: bên thua gặp email trùng, dùng lại tài khoản bên thắng.
            var winner = await userManager.FindByLoginAsync(loginProvider, providerKey);
            return winner is null
                ? new GoogleAccountResult(GoogleAccountOutcome.Failed, null)
                : new GoogleAccountResult(GoogleAccountOutcome.ExistingAccount, winner);
        }

        var roleAdded = await userManager.AddToRoleAsync(user, RoleNames.Student);
        var loginAdded = roleAdded.Succeeded
            ? await userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, loginProvider))
            : roleAdded;
        if (!loginAdded.Succeeded)
        {
            return new GoogleAccountResult(GoogleAccountOutcome.Failed, null);
        }

        await transaction.CommitAsync(cancellationToken);
        return new GoogleAccountResult(GoogleAccountOutcome.StudentCreated, user);
    }
}
