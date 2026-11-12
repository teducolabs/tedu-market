namespace TeduMarket.Domain.Common;

/// <summary>Vi phạm quy tắc nghiệp vụ; <see cref="ErrorCode"/> dạng UPPER_SNAKE_CASE, trả về client với mã 400.</summary>
public class DomainException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
