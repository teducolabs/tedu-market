using Microsoft.Extensions.Options;
using TeduMarket.Application.Notification;

namespace TeduMarket.Infrastructure.Notification;

internal sealed class StudentWebLinks(IOptions<NotificationOptions> options) : IStudentWebLinks
{
    public Uri CourseListUrl => new(EnsureTrailingSlash(options.Value.StudentWebUrl), "courses");

    // Không có "/" cuối thì Uri bỏ phân đoạn cuối khi ghép đường dẫn tương đối.
    private static Uri EnsureTrailingSlash(Uri uri) =>
        uri.AbsoluteUri.EndsWith('/') ? uri : new Uri(uri.AbsoluteUri + "/");
}
