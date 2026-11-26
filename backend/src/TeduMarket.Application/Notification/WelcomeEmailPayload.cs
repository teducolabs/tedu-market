using System.Text.Json;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Application.Notification;

/// <summary>Nội dung JSON của <see cref="OutboxMessage"/> loại <see cref="MessageType"/>.</summary>
public sealed record WelcomeEmailPayload(Guid UserId, string Email)
{
    public const string MessageType = "notification.welcome-email";

    /// <summary>Tạo message chào mừng; người gọi ghi nó cùng transaction với việc tạo tài khoản Học viên.</summary>
    public static OutboxMessage CreateOutboxMessage(Guid userId, string email, DateTimeOffset createdAt) =>
        OutboxMessage.Create(MessageType, JsonSerializer.Serialize(new WelcomeEmailPayload(userId, email)), createdAt);
}
