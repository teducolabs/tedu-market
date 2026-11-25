using Mediator;

namespace TeduMarket.Application.Identity.Commands.ChangePassword;

/// <summary>Admin đổi mật khẩu của chính mình (FR-42).</summary>
public sealed record ChangePasswordCommand(string? CurrentPassword, string? NewPassword) : ICommand<Unit>;
