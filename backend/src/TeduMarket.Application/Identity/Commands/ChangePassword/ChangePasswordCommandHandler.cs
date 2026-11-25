using Mediator;
using TeduMarket.Application.Common.Exceptions;

namespace TeduMarket.Application.Identity.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(ICurrentUser currentUser, IPasswordService passwordService)
    : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new ForbiddenException("Cần đăng nhập để đổi mật khẩu");

        var result = await passwordService.ChangePasswordAsync(userId, command.CurrentPassword!, command.NewPassword!, cancellationToken);
        return result.Outcome switch
        {
            PasswordChangeOutcome.Succeeded => Unit.Value,
            PasswordChangeOutcome.UserNotFound => throw new NotFoundException("Không tìm thấy tài khoản"),
            PasswordChangeOutcome.WrongCurrentPassword => throw new RequestValidationException(
                new Dictionary<string, string[]> { ["currentPassword"] = ["Mật khẩu hiện tại chưa đúng"] }),
            _ => throw new RequestValidationException(
                new Dictionary<string, string[]> { ["newPassword"] = [.. result.NewPasswordErrors] }),
        };
    }
}
