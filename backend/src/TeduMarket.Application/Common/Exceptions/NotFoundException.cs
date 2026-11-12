namespace TeduMarket.Application.Common.Exceptions;

/// <summary>Không có dữ liệu được yêu cầu; trả về client với mã 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);
