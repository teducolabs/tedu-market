using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Course;

internal static class CourseAccess
{
    /// <summary>Chỉ Admin hoặc Instructor được ghi Khóa học; vai trò khác bị từ chối (NFR-08).</summary>
    public static void EnsureCanWrite(ICurrentUser currentUser)
    {
        if (!currentUser.IsInRole(RoleNames.Admin) && !currentUser.IsInRole(RoleNames.Instructor))
        {
            throw new ForbiddenException("Chỉ Admin hoặc Instructor mới được quản lý Khóa học");
        }
    }

    /// <summary>Id Hồ sơ Instructor của người đang đăng nhập; <c>null</c> nếu là Admin hoặc chưa có hồ sơ.</summary>
    public static async Task<Guid?> FindOwnProfileIdAsync(
        ICurrentUser currentUser, IInstructorProfileReader profileReader, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole(RoleNames.Admin) || currentUser.UserId is not { } userId)
        {
            return null;
        }

        return (await profileReader.FindByUserIdAsync(userId, cancellationToken))?.InstructorProfileId;
    }

    /// <summary>Admin sửa/xóa mọi khóa; Instructor chỉ khóa thuộc Hồ sơ Instructor của mình, kiểm tra ở Handler chứ không chỉ ở UI (NFR-08).</summary>
    public static async Task EnsureCanModifyAsync(
        ICurrentUser currentUser, IInstructorProfileReader profileReader, Domain.Course.Course course, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        var ownProfileId = await FindOwnProfileIdAsync(currentUser, profileReader, cancellationToken);
        if (ownProfileId != course.InstructorProfileId)
        {
            throw new ForbiddenException("Instructor chỉ được thao tác trên khóa học của chính mình");
        }
    }

    public static async Task EnsureCategoryExistsAsync(
        ICategoryRepository categoryRepository, Guid categoryId, CancellationToken cancellationToken)
    {
        if (await categoryRepository.FindAsync(categoryId, cancellationToken) is null)
        {
            throw new RequestValidationException(new Dictionary<string, string[]> { ["categoryId"] = ["Category không tồn tại"] });
        }
    }

    public static CourseDto ToDto(Domain.Course.Course course, IThumbnailStorage thumbnailStorage) => new(
        course.CourseId, course.InstructorProfileId, course.CategoryId, course.Title, course.Slug, course.Description, course.Price, course.Status.ToString(),
        course.ThumbnailKey is null ? null : thumbnailStorage.GetPublicUrl(course.ThumbnailKey));
}
