using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Web;
using TeduMarket.Infrastructure.Course;

namespace TeduMarket.Api.IntegrationTests.Course;

/// <summary>
/// Giả MinIO ở biên HTTP: nhận PUT/HEAD/DELETE vào <c>/thumbnails/{khóa}</c>, tính lại chữ ký SigV4 từ request thật
/// (kiểu và dung lượng lấy từ header gửi đi) và trả 403 khi lệch — giống cách MinIO từ chối file sai kiểu/dung lượng.
/// </summary>
internal sealed class FakeMinioBackchannel : HttpMessageHandler
{
    public const string AccessKey = "test-access-key";
    public const string SecretKey = "test-secret-key";
    public const string Bucket = "thumbnails";

    private static readonly Uri _endpoint = new("http://localhost:9000");

    public ConcurrentDictionary<string, byte[]> Objects { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        var query = HttpUtility.ParseQueryString(uri.Query);
        var signedAt = DateTimeOffset.ParseExact(query["X-Amz-Date"]!, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        var headers = new Dictionary<string, string>();
        if (request.Method == HttpMethod.Put)
        {
            headers["content-type"] = request.Content!.Headers.ContentType!.MediaType!;
            headers["content-length"] = request.Content.Headers.ContentLength!.Value.ToString(CultureInfo.InvariantCulture);
        }

        var expected = new PresignedUrlSigner(AccessKey, SecretKey, "us-east-1")
            .Presign(request.Method.Method, _endpoint, uri.AbsolutePath, headers, TimeSpan.FromSeconds(int.Parse(query["X-Amz-Expires"]!, CultureInfo.InvariantCulture)), signedAt);
        if (expected.AbsoluteUri != uri.AbsoluteUri)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden));
        }

        var key = Uri.UnescapeDataString(uri.AbsolutePath)[$"/{Bucket}/".Length..];
        if (request.Method == HttpMethod.Put)
        {
            Objects[key] = [];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        if (request.Method == HttpMethod.Delete)
        {
            Objects.TryRemove(key, out _);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }

        return Task.FromResult(new HttpResponseMessage(Objects.ContainsKey(key) ? HttpStatusCode.OK : HttpStatusCode.NotFound));
    }
}
