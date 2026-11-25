using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Identity.Commands.ChangePassword;

namespace TeduMarket.Application.Tests.Identity;

[Trait("Category", "Unit")]
public sealed class ChangePasswordCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_Succeeded_PassesCurrentUserAndPasswordsToServiceAsync()
    {
        // Arrange
        var service = new FakePasswordService(PasswordChangeResult.Of(PasswordChangeOutcome.Succeeded));
        var handler = new ChangePasswordCommandHandler(new FakeCurrentUser(_userId), service);

        // Act
        await handler.Handle(new ChangePasswordCommand("cu", "moi"), CancellationToken.None);

        // Assert
        Assert.Equal((_userId, "cu", "moi"), service.Call);
    }

    [Fact]
    public async Task Handle_Guest_ThrowsForbiddenWithoutCallingServiceAsync()
    {
        // Arrange
        var service = new FakePasswordService(PasswordChangeResult.Of(PasswordChangeOutcome.Succeeded));
        var handler = new ChangePasswordCommandHandler(new FakeCurrentUser(null), service);

        // Act
        var act = async () => await handler.Handle(new ChangePasswordCommand("cu", "moi"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Null(service.Call);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new ChangePasswordCommandHandler(new FakeCurrentUser(_userId), new FakePasswordService(PasswordChangeResult.Of(PasswordChangeOutcome.UserNotFound)));

        // Act
        var act = async () => await handler.Handle(new ChangePasswordCommand("cu", "moi"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ThrowsValidationOnCurrentPasswordAsync()
    {
        // Arrange
        var handler = new ChangePasswordCommandHandler(new FakeCurrentUser(_userId), new FakePasswordService(PasswordChangeResult.Of(PasswordChangeOutcome.WrongCurrentPassword)));

        // Act
        var act = async () => await handler.Handle(new ChangePasswordCommand("cu", "moi"), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.Contains("currentPassword", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_NewPasswordRejected_ThrowsValidationOnNewPasswordWithServiceMessagesAsync()
    {
        // Arrange
        var rejected = new PasswordChangeResult(PasswordChangeOutcome.NewPasswordRejected, ["Quá ngắn"]);
        var handler = new ChangePasswordCommandHandler(new FakeCurrentUser(_userId), new FakePasswordService(rejected));

        // Act
        var act = async () => await handler.Handle(new ChangePasswordCommand("cu", "moi"), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.Equal(["Quá ngắn"], exception.Errors["newPassword"]);
    }

    private sealed class FakeCurrentUser(Guid? userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public bool IsInRole(string roleName) => false;
    }

    private sealed class FakePasswordService(PasswordChangeResult result) : IPasswordService
    {
        public (Guid UserId, string Current, string New)? Call { get; private set; }

        public Task<PasswordChangeResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            Call = (userId, currentPassword, newPassword);
            return Task.FromResult(result);
        }
    }
}
