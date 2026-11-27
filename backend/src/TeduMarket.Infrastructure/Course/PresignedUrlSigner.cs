using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TeduMarket.Infrastructure.Course;

/// <summary>Ký presigned URL theo AWS Signature Version 4 (tham số trên query, payload <c>UNSIGNED-PAYLOAD</c>) cho kho lưu trữ tương thích S3.</summary>
public sealed class PresignedUrlSigner(string accessKey, string secretKey, string region)
{
    private const string Algorithm = "AWS4-HMAC-SHA256";

    /// <param name="method">GET, HEAD, PUT...</param>
    /// <param name="endpoint">Gốc url (scheme, host, port); host nằm trong chữ ký nên client phải gọi đúng host này.</param>
    /// <param name="path">Đường dẫn bắt đầu bằng <c>/</c>, chưa mã hóa.</param>
    /// <param name="signedHeaders">Header ngoài <c>host</c> bị buộc phải khớp (tên viết thường), ví dụ <c>content-type</c>, <c>content-length</c>.</param>
    /// <param name="lifetime">Thời gian url còn hiệu lực.</param>
    /// <param name="now">Thời điểm ký.</param>
    public Uri Presign(
        string method, Uri endpoint, string path, IReadOnlyDictionary<string, string> signedHeaders, TimeSpan lifetime, DateTimeOffset now)
    {
        var utc = now.UtcDateTime;
        var amzDate = utc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var scopeDate = utc.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var scope = $"{scopeDate}/{region}/s3/aws4_request";

        var headers = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["host"] = endpoint.IsDefaultPort ? endpoint.Host : $"{endpoint.Host}:{endpoint.Port}",
        };
        foreach (var (name, value) in signedHeaders)
        {
            headers[name.ToLowerInvariant()] = value.Trim();
        }

        var headerNames = string.Join(';', headers.Keys);
        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["X-Amz-Algorithm"] = Algorithm,
            ["X-Amz-Credential"] = $"{accessKey}/{scope}",
            ["X-Amz-Date"] = amzDate,
            ["X-Amz-Expires"] = ((int)lifetime.TotalSeconds).ToString(CultureInfo.InvariantCulture),
            ["X-Amz-SignedHeaders"] = headerNames,
        };

        var canonicalPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
        var canonicalQuery = string.Join('&', query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        var canonicalHeaders = string.Concat(headers.Select(pair => $"{pair.Key}:{pair.Value}\n"));
        var canonicalRequest = $"{method}\n{canonicalPath}\n{canonicalQuery}\n{canonicalHeaders}\n{headerNames}\nUNSIGNED-PAYLOAD";

        var stringToSign = $"{Algorithm}\n{amzDate}\n{scope}\n{Hex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))}";
        var signingKey = Hmac(Hmac(Hmac(Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), scopeDate), region), "s3"), "aws4_request");
        var signature = Hex(Hmac(signingKey, stringToSign));

        return new Uri($"{endpoint.GetLeftPart(UriPartial.Authority)}{canonicalPath}?{canonicalQuery}&X-Amz-Signature={signature}");
    }

    private static byte[] Hmac(byte[] key, string data) => HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(data));

    private static string Hex(byte[] bytes) => Convert.ToHexStringLower(bytes);
}
