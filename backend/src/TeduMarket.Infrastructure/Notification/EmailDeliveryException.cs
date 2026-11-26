namespace TeduMarket.Infrastructure.Notification;

/// <summary>Resend không nhận email (chưa cấu hình, lỗi mạng hoặc phản hồi không thành công).</summary>
public sealed class EmailDeliveryException : Exception
{
    public EmailDeliveryException(string message)
        : base(message)
    {
    }

    public EmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
