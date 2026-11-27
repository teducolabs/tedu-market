using System.Text.RegularExpressions;

namespace TeduMarket.Application.Course;

internal static partial class ThumbnailRules
{
    /// <summary>Dung lượng tối đa của ảnh thumbnail: 2 MB.</summary>
    public const long MaxContentLength = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> ExtensionByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png",
        ["image/webp"] = "webp",
    };

    public static bool IsAllowedContentType(string? contentType) =>
        contentType is not null && ExtensionByContentType.ContainsKey(contentType);

    /// <summary>Khóa mới dạng <c>{courseId}/{guid}.{đuôi}</c>; tên sinh bằng GUID nên không đoán được và không trùng.</summary>
    public static string NewObjectKey(Guid courseId, string contentType) =>
        $"{courseId:N}/{Guid.CreateVersion7():N}.{ExtensionByContentType[contentType]}";

    /// <summary>Khóa có đúng dạng do <see cref="NewObjectKey"/> sinh và thuộc đúng khóa học này.</summary>
    public static bool IsObjectKeyOfCourse(string? objectKey, Guid courseId) =>
        objectKey is not null && ObjectKeyPattern().Match(objectKey) is { Success: true } match
        && match.Groups["course"].Value == courseId.ToString("N");

    [GeneratedRegex("^(?<course>[0-9a-f]{32})/[0-9a-f]{32}\\.(jpg|png|webp)$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 200)]
    private static partial Regex ObjectKeyPattern();
}
