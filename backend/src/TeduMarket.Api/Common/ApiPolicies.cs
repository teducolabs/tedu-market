using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TeduMarket.Api.Common;

/// <summary>Cấu hình CORS, Output Cache và RateLimiter trong bộ nhớ (T06; NFR-01, NFR-09, NFR-10).</summary>
public static class ApiPolicies
{
    public const string CorsPolicy = "AllowedOrigins";
    public const string CatalogCachePolicy = "Catalog";
    public const string CatalogCacheTag = "catalog";
    public const string CheckoutRateLimitPolicy = "Checkout";
    public const int CheckoutPermitLimit = 10;

    private const string _allowedOriginsKey = "Cors:AllowedOrigins";
    private static readonly TimeSpan _catalogCacheDuration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan _checkoutWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddApiPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = ParseOrigins(configuration[_allowedOriginsKey]);

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

        services.AddOutputCache(options => options.AddPolicy(CatalogCachePolicy, builder => builder
            .Expire(_catalogCacheDuration)
            .SetVaryByQuery("*")
            .Tag(CatalogCacheTag)));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(CheckoutRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                GetPartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = CheckoutPermitLimit,
                    Window = _checkoutWindow,
                    QueueLimit = 0,
                }));
            options.OnRejected = WriteRateLimitedAsync;
        });

        return services;
    }

    /// <summary>Khóa phân vùng rate limit: Id người dùng đã đăng nhập; chưa đăng nhập thì theo địa chỉ IP.</summary>
    public static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId)
            ? $"ip:{context.Connection.RemoteIpAddress}"
            : $"user:{userId}";
    }

    public static string[] ParseOrigins(string? value) =>
        (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static async ValueTask WriteRateLimitedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        var problem = new ProblemDetails
        {
            Type = "https://httpstatuses.io/429",
            Title = "Bạn thao tác quá nhanh, vui lòng thử lại sau",
            Status = StatusCodes.Status429TooManyRequests,
        };
        problem.Extensions["errorCode"] = "RATE_LIMITED";
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
    }
}
