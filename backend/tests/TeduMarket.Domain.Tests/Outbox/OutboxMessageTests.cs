using TeduMarket.Domain.Common;
using TeduMarket.Domain.Outbox;

namespace TeduMarket.Domain.Tests.Outbox;

public sealed class OutboxMessageTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidInput_ReturnsPendingMessageDueImmediately()
    {
        // Arrange & Act
        var message = OutboxMessage.Create("welcome-email", "{}", _now);

        // Assert
        Assert.NotEqual(Guid.Empty, message.OutboxMessageId);
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(_now, message.NextAttemptAt);
        Assert.Equal(0, message.AttemptCount);
        Assert.Null(message.ProcessedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankType_ThrowsDomainException(string type)
    {
        // Arrange & Act
        var exception = Assert.Throws<DomainException>(() => OutboxMessage.Create(type, "{}", _now));

        // Assert
        Assert.Equal("INVALID_OUTBOX_MESSAGE_TYPE", exception.ErrorCode);
    }

    [Fact]
    public void Create_TypeTooLong_ThrowsDomainException()
    {
        // Arrange
        var type = new string('a', OutboxMessage.TypeMaxLength + 1);

        // Act
        var exception = Assert.Throws<DomainException>(() => OutboxMessage.Create(type, "{}", _now));

        // Assert
        Assert.Equal("INVALID_OUTBOX_MESSAGE_TYPE", exception.ErrorCode);
    }

    [Fact]
    public void Create_BlankPayload_ThrowsDomainException()
    {
        // Arrange & Act
        var exception = Assert.Throws<DomainException>(() => OutboxMessage.Create("welcome-email", " ", _now));

        // Assert
        Assert.Equal("INVALID_OUTBOX_MESSAGE_PAYLOAD", exception.ErrorCode);
    }

    [Fact]
    public void MarkProcessed_PendingMessage_SetsProcessedAt()
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);

        // Act
        message.MarkProcessed(_now.AddSeconds(2));

        // Assert
        Assert.Equal(OutboxMessageStatus.Processed, message.Status);
        Assert.Equal(_now.AddSeconds(2), message.ProcessedAt);
    }

    [Fact]
    public void MarkProcessed_AfterEarlierFailure_ClearsLastError()
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);
        message.MarkFailed("lỗi", _now);

        // Act
        message.MarkProcessed(_now.AddSeconds(10));

        // Assert
        Assert.Null(message.LastError);
        Assert.Equal(1, message.AttemptCount);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 20)]
    [InlineData(4, 40)]
    public void MarkFailed_UnderMaxAttempts_StaysPendingWithExponentialBackoff(int failures, int expectedDelaySeconds)
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);

        // Act
        for (var i = 0; i < failures; i++)
        {
            message.MarkFailed("lỗi", _now);
        }

        // Assert
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(failures, message.AttemptCount);
        Assert.Equal(_now.AddSeconds(expectedDelaySeconds), message.NextAttemptAt);
        Assert.Equal("lỗi", message.LastError);
    }

    [Fact]
    public void MarkFailed_ReachesMaxAttempts_BecomesFailed()
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);

        // Act
        for (var i = 0; i < OutboxMessage.MaxAttemptCount; i++)
        {
            message.MarkFailed("lỗi", _now);
        }

        // Assert
        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(OutboxMessage.MaxAttemptCount, message.AttemptCount);
        Assert.Null(message.ProcessedAt);
    }

    [Fact]
    public void MarkFailed_ErrorTooLong_TruncatesLastError()
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);

        // Act
        message.MarkFailed(new string('x', OutboxMessage.LastErrorMaxLength + 50), _now);

        // Assert
        Assert.Equal(OutboxMessage.LastErrorMaxLength, message.LastError!.Length);
    }

    [Fact]
    public void MarkFailed_AlreadyProcessed_ThrowsDomainException()
    {
        // Arrange
        var message = OutboxMessage.Create("welcome-email", "{}", _now);
        message.MarkProcessed(_now);

        // Act
        var exception = Assert.Throws<DomainException>(() => message.MarkFailed("lỗi", _now));

        // Assert
        Assert.Equal("OUTBOX_MESSAGE_NOT_PENDING", exception.ErrorCode);
    }
}
