namespace TeduMarket.Application.Notification;

/// <summary>Biên ra dịch vụ gửi email (Resend). Ném exception khi gửi lỗi để Outbox giữ message và gửi lại.</summary>
public interface IEmailSender
{
    /// <param name="message">Email cần gửi.</param>
    /// <param name="idempotencyKey">Khóa chống gửi trùng khi Outbox giao lại cùng một message.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    Task SendAsync(EmailMessage message, string idempotencyKey, CancellationToken cancellationToken);
}
