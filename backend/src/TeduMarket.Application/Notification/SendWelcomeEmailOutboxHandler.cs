using System.Net;
using System.Text.Json;
using TeduMarket.Application.Common.Outbox;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Application.Notification;

/// <summary>
/// Gửi <see cref="WelcomeEmailPayload"/> (FR-27). Idempotent: khóa chống trùng là id của message, nên giao lại sau sự cố
/// không làm Học viên nhận hai email.
/// </summary>
public sealed class SendWelcomeEmailOutboxHandler(IEmailSender emailSender, IStudentWebLinks links) : IOutboxMessageHandler
{
    public string MessageType => WelcomeEmailPayload.MessageType;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<WelcomeEmailPayload>(message.Payload)
            ?? throw new InvalidOperationException($"Nội dung message {message.OutboxMessageId} không hợp lệ");
        var email = BuildEmail(payload.Email, links.CourseListUrl);
        await emailSender.SendAsync(email, message.OutboxMessageId.ToString(), cancellationToken);
    }

    private static EmailMessage BuildEmail(string recipient, Uri courseListUrl)
    {
        const string subject = "Chào mừng bạn đến với TEDU Market";
        var address = WebUtility.HtmlEncode(recipient);
        var link = WebUtility.HtmlEncode(courseListUrl.AbsoluteUri);
        var html = $"""
            <p>Chào bạn,</p>
            <p>Tài khoản Học viên của bạn đã được tạo bằng email Google <strong>{address}</strong>.</p>
            <p><a href="{link}">Xem các khóa học</a></p>
            """;
        var text = $"""
            Chào bạn,

            Tài khoản Học viên của bạn đã được tạo bằng email Google {recipient}.

            Xem các khóa học: {courseListUrl.AbsoluteUri}
            """;
        return new EmailMessage(recipient, subject, html, text);
    }
}
