using Serilog.Context;

namespace TeduMarket.Api.Common;

/// <summary>Gắn CorrelationId vào mọi request: nhận từ header <c>X-Correlation-Id</c> nếu hợp lệ, không thì dùng TraceIdentifier; trả lại qua header response và đẩy vào log.</summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string PropertyName = "CorrelationId";
    public const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadValidHeader(context) ?? context.TraceIdentifier;
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(PropertyName, correlationId))
        {
            await next(context);
        }
    }

    private static string? ReadValidHeader(HttpContext context)
    {
        var value = context.Request.Headers[HeaderName].ToString();
        return value.Length is > 0 and <= MaxLength && value.All(IsAllowed) ? value : null;
    }

    private static bool IsAllowed(char character) => char.IsAsciiLetterOrDigit(character) || character is '-' or '_';
}
