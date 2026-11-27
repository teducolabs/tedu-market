using TeduMarket.Infrastructure.Course;

namespace TeduMarket.Api.IntegrationTests.Course;

[Trait("Category", "Unit")]
public sealed class PresignedUrlSignerTests
{
    private static readonly DateTimeOffset _signedAt = new(2013, 5, 24, 0, 0, 0, TimeSpan.Zero);

    private readonly PresignedUrlSigner _signer = new("AKIAIOSFODNN7EXAMPLE", "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY", "us-east-1");

    [Fact]
    public void Presign_AwsDocumentedGetExample_ProducesDocumentedSignature()
    {
        // Arrange (ví dụ "Authenticating Requests: Using Query Parameters" của tài liệu AWS S3)
        var endpoint = new Uri("https://examplebucket.s3.amazonaws.com");

        // Act
        var url = _signer.Presign("GET", endpoint, "/test.txt", new Dictionary<string, string>(), TimeSpan.FromSeconds(86400), _signedAt);

        // Assert
        Assert.Equal(
            "https://examplebucket.s3.amazonaws.com/test.txt?X-Amz-Algorithm=AWS4-HMAC-SHA256"
            + "&X-Amz-Credential=AKIAIOSFODNN7EXAMPLE%2F20130524%2Fus-east-1%2Fs3%2Faws4_request"
            + "&X-Amz-Date=20130524T000000Z&X-Amz-Expires=86400&X-Amz-SignedHeaders=host"
            + "&X-Amz-Signature=aeeed9bbccd4d02ee5c0109b86d86835f995330da4c265957d157751f604d404",
            url.AbsoluteUri);
    }

    [Fact]
    public void Presign_PutWithTypeAndLength_SignsThoseHeaders()
    {
        // Arrange
        var headers = new Dictionary<string, string> { ["content-type"] = "image/png", ["content-length"] = "1024" };

        // Act
        var url = _signer.Presign("PUT", new Uri("http://localhost:9000"), "/thumbnails/a/b.png", headers, TimeSpan.FromMinutes(5), _signedAt);

        // Assert
        Assert.Contains("X-Amz-SignedHeaders=content-length%3Bcontent-type%3Bhost", url.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal("localhost", url.Host);
        Assert.Equal(9000, url.Port);
        Assert.Equal("/thumbnails/a/b.png", url.AbsolutePath);
    }

    [Fact]
    public void Presign_DifferentContentLength_ChangesSignature()
    {
        // Arrange
        var small = new Dictionary<string, string> { ["content-type"] = "image/png", ["content-length"] = "1024" };
        var large = new Dictionary<string, string> { ["content-type"] = "image/png", ["content-length"] = "9999999" };
        var endpoint = new Uri("http://localhost:9000");

        // Act
        var first = _signer.Presign("PUT", endpoint, "/thumbnails/a.png", small, TimeSpan.FromMinutes(5), _signedAt);
        var second = _signer.Presign("PUT", endpoint, "/thumbnails/a.png", large, TimeSpan.FromMinutes(5), _signedAt);

        // Assert
        Assert.NotEqual(first.Query, second.Query);
    }
}
