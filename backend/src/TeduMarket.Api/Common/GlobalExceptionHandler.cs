using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Domain.Common;

namespace TeduMarket.Api.Common;

/// <summary>Đổi mọi exception thành ProblemDetails (RFC 9457); lỗi không lường chỉ trả INTERNAL_ERROR, không lộ stack hay thông điệp.</summary>
public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;
        var problem = exception switch
        {
            RequestValidationException validation => Create(StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ", "INVALID_REQUEST", correlationId, validation.Errors),
            DomainException domain => Create(StatusCodes.Status400BadRequest, domain.Message, domain.ErrorCode, correlationId),
            NotFoundException notFound => Create(StatusCodes.Status404NotFound, notFound.Message, "NOT_FOUND", correlationId),
            ForbiddenException forbidden => Create(StatusCodes.Status403Forbidden, forbidden.Message, "FORBIDDEN", correlationId),
            _ => Create(StatusCodes.Status500InternalServerError, "Có lỗi xảy ra, vui lòng thử lại", "INTERNAL_ERROR", correlationId),
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            LogUnhandled(logger, exception, correlationId);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static ProblemDetails Create(int status, string title, string errorCode, string correlationId, IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
        };
        problem.Extensions["errorCode"] = errorCode;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        problem.Extensions["correlationId"] = correlationId;
        return problem;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Lỗi không xử lý, correlationId {CorrelationId}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string correlationId);
}
