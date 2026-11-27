using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeduMarket.Application.Course;

namespace TeduMarket.Infrastructure.Course;

/// <summary>Kho lưu trữ thumbnail trên MinIO (S3), truy cập path-style <c>/{bucket}/{objectKey}</c> (FR-23).</summary>
public sealed partial class MinioThumbnailStorage(IOptions<MinioOptions> options, IHttpClientFactory httpClientFactory, TimeProvider timeProvider, ILogger<MinioThumbnailStorage> logger)
    : IThumbnailStorage
{
    public const string HttpClientName = "minio-storage";

    private static readonly TimeSpan HeadLifetime = TimeSpan.FromMinutes(1);

    public ThumbnailUploadTicket CreateUpload(string objectKey, string contentType, long contentLength)
    {
        var settings = options.Value;
        var lifetime = TimeSpan.FromSeconds(settings.UploadUrlLifetimeSeconds);
        var now = timeProvider.GetUtcNow();
        var headers = new Dictionary<string, string>
        {
            ["content-type"] = contentType,
            ["content-length"] = contentLength.ToString(CultureInfo.InvariantCulture),
        };
        var url = CreateSigner().Presign("PUT", settings.PublicEndpoint ?? settings.Endpoint, ObjectPath(objectKey), headers, lifetime, now);
        return new ThumbnailUploadTicket(url.AbsoluteUri, now + lifetime);
    }

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var url = CreateSigner().Presign(
            "HEAD", settings.Endpoint, ObjectPath(objectKey), new Dictionary<string, string>(), HeadLifetime, timeProvider.GetUtcNow());
        using var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var url = CreateSigner().Presign(
            "DELETE", settings.Endpoint, ObjectPath(objectKey), new Dictionary<string, string>(), HeadLifetime, timeProvider.GetUtcNow());
        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            LogDeleteFailed(exception, objectKey);
        }
    }

    public string GetPublicUrl(string objectKey)
    {
        var settings = options.Value;
        var origin = (settings.PublicEndpoint ?? settings.Endpoint).GetLeftPart(UriPartial.Authority);
        return $"{origin}{string.Join('/', ObjectPath(objectKey).Split('/').Select(Uri.EscapeDataString))}";
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Không xóa được thumbnail cũ {ObjectKey}; ảnh bị bỏ lại trong kho lưu trữ")]
    private partial void LogDeleteFailed(Exception exception, string objectKey);

    private string ObjectPath(string objectKey) => $"/{options.Value.Bucket}/{objectKey}";

    private PresignedUrlSigner CreateSigner()
    {
        var settings = options.Value;
        if (string.IsNullOrEmpty(settings.AccessKey) || string.IsNullOrEmpty(settings.SecretKey))
        {
            throw new InvalidOperationException("Thiếu cấu hình Minio:AccessKey hoặc Minio:SecretKey");
        }

        return new PresignedUrlSigner(settings.AccessKey, settings.SecretKey, settings.Region);
    }
}
