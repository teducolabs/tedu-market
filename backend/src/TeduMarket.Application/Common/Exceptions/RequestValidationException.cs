namespace TeduMarket.Application.Common.Exceptions;

/// <summary>Dữ liệu vào sai; <see cref="Errors"/> là map tên trường camelCase → các thông điệp lỗi. Trả về client với mã 400.</summary>
public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Dữ liệu không hợp lệ")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
