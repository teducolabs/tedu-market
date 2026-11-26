using System.Text.Json;
using TeduMarket.Application.Notification;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Application.Tests.Notification;

[Trait("Category", "Unit")]
public sealed class SendWelcomeEmailOutboxHandlerTests
{
    private static readonly Uri _courseListUrl = new("https://hoc.example.test/courses");
    private static readonly DateTimeOffset _now = new(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ValidPayload_SendsEmailToStudentWithCourseListLinkAsync()
    {
        // Arrange
        var sender = new FakeEmailSender();
        var handler = new SendWelcomeEmailOutboxHandler(sender, new FakeLinks(_courseListUrl));
        var message = WelcomeEmailPayload.CreateOutboxMessage(Guid.NewGuid(), "hoc.vien@example.test", _now);

        // Act
        await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        var sent = Assert.Single(sender.Sent);
        Assert.Equal("hoc.vien@example.test", sent.Message.To);
        Assert.Contains("hoc.vien@example.test", sent.Message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("href=\"https://hoc.example.test/courses\"", sent.Message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("https://hoc.example.test/courses", sent.Message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_SameMessageDeliveredTwice_UsesSameIdempotencyKeyAsync()
    {
        // Arrange
        var sender = new FakeEmailSender();
        var handler = new SendWelcomeEmailOutboxHandler(sender, new FakeLinks(_courseListUrl));
        var message = WelcomeEmailPayload.CreateOutboxMessage(Guid.NewGuid(), "hoc.vien@example.test", _now);

        // Act
        await handler.HandleAsync(message, CancellationToken.None);
        await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        Assert.All(sender.Sent, sent => Assert.Equal(message.OutboxMessageId.ToString(), sent.IdempotencyKey));
    }

    [Fact]
    public async Task Handle_EmailContainsHtml_EncodesItInHtmlBodyAsync()
    {
        // Arrange
        var sender = new FakeEmailSender();
        var handler = new SendWelcomeEmailOutboxHandler(sender, new FakeLinks(_courseListUrl));
        var message = WelcomeEmailPayload.CreateOutboxMessage(Guid.NewGuid(), "a<b>@example.test", _now);

        // Act
        await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        var sent = Assert.Single(sender.Sent);
        Assert.DoesNotContain("<b>", sent.Message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("a&lt;b&gt;@example.test", sent.Message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_SenderFails_PropagatesExceptionSoOutboxKeepsMessageAsync()
    {
        // Arrange
        var sender = new FakeEmailSender { Failure = new InvalidOperationException("Resend lỗi") };
        var handler = new SendWelcomeEmailOutboxHandler(sender, new FakeLinks(_courseListUrl));
        var message = WelcomeEmailPayload.CreateOutboxMessage(Guid.NewGuid(), "hoc.vien@example.test", _now);

        // Act
        var act = async () => await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task Handle_PayloadIsJsonNull_ThrowsWithoutSendingAsync()
    {
        // Arrange
        var sender = new FakeEmailSender();
        var handler = new SendWelcomeEmailOutboxHandler(sender, new FakeLinks(_courseListUrl));
        var message = OutboxMessage.Create(WelcomeEmailPayload.MessageType, "null", _now);

        // Act
        var act = async () => await handler.HandleAsync(message, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public void MessageType_Handler_MatchesPayloadMessageType()
    {
        // Arrange
        var handler = new SendWelcomeEmailOutboxHandler(new FakeEmailSender(), new FakeLinks(_courseListUrl));

        // Act
        var messageType = handler.MessageType;

        // Assert
        Assert.Equal(WelcomeEmailPayload.MessageType, messageType);
    }

    [Fact]
    public void CreateOutboxMessage_StudentAndEmail_ProducesPendingMessageWithRoundTrippablePayload()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var message = WelcomeEmailPayload.CreateOutboxMessage(userId, "hoc.vien@example.test", _now);

        // Assert
        Assert.Equal(WelcomeEmailPayload.MessageType, message.Type);
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(new WelcomeEmailPayload(userId, "hoc.vien@example.test"), JsonSerializer.Deserialize<WelcomeEmailPayload>(message.Payload));
    }

    private sealed class FakeLinks(Uri courseListUrl) : IStudentWebLinks
    {
        public Uri CourseListUrl => courseListUrl;
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<(EmailMessage Message, string IdempotencyKey)> Sent { get; } = [];

        public Exception? Failure { get; init; }

        public Task SendAsync(EmailMessage message, string idempotencyKey, CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            Sent.Add((message, idempotencyKey));
            return Task.CompletedTask;
        }
    }
}
