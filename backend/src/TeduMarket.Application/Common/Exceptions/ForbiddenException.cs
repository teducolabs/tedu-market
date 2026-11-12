namespace TeduMarket.Application.Common.Exceptions;

/// <summary>Sai quyền sở hữu hoặc role; trả về client với mã 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
