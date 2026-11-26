using TeduMarket.Domain.Common;

namespace TeduMarket.Domain.Outbox;

/// <summary>
/// Việc nền ghi cùng transaction với thay đổi nghiệp vụ (Outbox Pattern, NFR-05); BackgroundService đọc và xử lý sau khi transaction commit.
/// Lỗi thì tăng <see cref="AttemptCount"/> và retry theo backoff; đủ <see cref="MaxAttemptCount"/> lần thì chuyển <see cref="OutboxMessageStatus.Failed"/>.
/// </summary>
public sealed class OutboxMessage
{
    public const int MaxAttemptCount = 5;
    public const int TypeMaxLength = 200;
    public const int LastErrorMaxLength = 2000;

    /// <summary>Thời gian chờ trước lần retry đầu; nhân đôi sau mỗi lần lỗi tiếp theo.</summary>
    public static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(5);

    private OutboxMessage()
    {
    }

    public Guid OutboxMessageId { get; private set; }

    /// <summary>Loại message, để chọn handler xử lý.</summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>Nội dung JSON của message.</summary>
    public string Payload { get; private set; } = string.Empty;

    public OutboxMessageStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Thời điểm sớm nhất được thử xử lý (dùng cho backoff).</summary>
    public DateTimeOffset NextAttemptAt { get; private set; }

    /// <summary>Chỉ đặt sau khi xử lý thành công.</summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public static OutboxMessage Create(string type, string payload, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Length > TypeMaxLength)
        {
            throw new DomainException("INVALID_OUTBOX_MESSAGE_TYPE", $"Loại message phải có từ 1 đến {TypeMaxLength} ký tự");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new DomainException("INVALID_OUTBOX_MESSAGE_PAYLOAD", "Nội dung message không được để trống");
        }

        return new OutboxMessage
        {
            OutboxMessageId = Guid.CreateVersion7(),
            Type = type,
            Payload = payload,
            Status = OutboxMessageStatus.Pending,
            CreatedAt = createdAt,
            NextAttemptAt = createdAt,
        };
    }

    public void MarkProcessed(DateTimeOffset now)
    {
        EnsurePending();
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = now;
        LastError = null;
    }

    public void MarkFailed(string error, DateTimeOffset now)
    {
        EnsurePending();
        AttemptCount++;
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;
        if (AttemptCount >= MaxAttemptCount)
        {
            Status = OutboxMessageStatus.Failed;
            return;
        }

        NextAttemptAt = now + (BaseRetryDelay * Math.Pow(2, AttemptCount - 1));
    }

    private void EnsurePending()
    {
        if (Status != OutboxMessageStatus.Pending)
        {
            throw new DomainException("OUTBOX_MESSAGE_NOT_PENDING", "Message không còn ở trạng thái chờ xử lý");
        }
    }
}
