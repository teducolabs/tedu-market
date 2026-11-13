using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TeduMarket.Infrastructure.Health;

/// <summary>Kiểm tra MinIO sống qua <c>/minio/health/live</c> (không cần khóa truy cập).</summary>
public sealed class MinioHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HttpClientName = "minio-health";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(new Uri("/minio/health/live", UriKind.Relative), cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"MinIO trả {(int)response.StatusCode}");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            return HealthCheckResult.Unhealthy("MinIO không phản hồi", exception);
        }
    }
}
