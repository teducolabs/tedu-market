using Mediator;
using TeduMarket.Api.Common;
using TeduMarket.Application.Course;
using TeduMarket.Application.Course.Commands.AddLesson;
using TeduMarket.Application.Course.Commands.CreateCourse;
using TeduMarket.Application.Course.Commands.CreateThumbnailUpload;
using TeduMarket.Application.Course.Commands.DeleteCourse;
using TeduMarket.Application.Course.Commands.DeleteLesson;
using TeduMarket.Application.Course.Commands.ReorderLessons;
using TeduMarket.Application.Course.Commands.SetCourseThumbnail;
using TeduMarket.Application.Course.Commands.SetLessonPreview;
using TeduMarket.Application.Course.Commands.UpdateCourse;
using TeduMarket.Application.Course.Commands.UpdateLesson;
using TeduMarket.Application.Course.Queries.ListLessons;

namespace TeduMarket.Api.Course;

/// <summary>Instructor và Admin tạo, sửa Khóa học ở trạng thái Chưa đăng (<c>/api/v1/courses</c>, FR-19, FR-22), Instructor/Admin xóa khóa; Bài học của khóa nằm dưới <c>/courses/{courseId}/lessons</c> (T27, FR-20, FR-45); quyền sở hữu kiểm tra trong Handler.</summary>
public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/courses").WithTags("Courses");

        group.MapPost("/", async (CreateCourseRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var course = await mediator.Send(
                    new CreateCourseCommand(request.InstructorProfileId, request.CategoryId, request.Title, request.Description, request.Price),
                    cancellationToken);
                return Results.Created($"/api/v1/courses/{course.CourseId}", course);
            })
            .Produces<CourseDto>(StatusCodes.Status201Created)
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{courseId:guid}", async (Guid courseId, UpdateCourseRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(
                    new UpdateCourseCommand(courseId, request.CategoryId, request.Title, request.Description, request.Price),
                    cancellationToken)))
            .Produces<CourseDto>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapDelete("/{courseId:guid}", async (Guid courseId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteCourseCommand(courseId), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPost("/{courseId:guid}/thumbnail-upload", async (Guid courseId, ThumbnailUploadRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new CreateThumbnailUploadCommand(courseId, request.ContentType, request.ContentLength), cancellationToken)))
            .Produces<ThumbnailUploadDto>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{courseId:guid}/thumbnail", async (Guid courseId, SetThumbnailRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new SetCourseThumbnailCommand(courseId, request.ObjectKey), cancellationToken)))
            .Produces<CourseDto>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapGet("/{courseId:guid}/lessons", async (Guid courseId, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new ListLessonsQuery(courseId), cancellationToken)))
            .Produces<IReadOnlyList<LessonDto>>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPost("/{courseId:guid}/lessons", async (Guid courseId, LessonRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var lesson = await mediator.Send(new AddLessonCommand(courseId, request.LessonTitle), cancellationToken);
                return Results.Created($"/api/v1/courses/{courseId}/lessons/{lesson.LessonId}", lesson);
            })
            .Produces<LessonDto>(StatusCodes.Status201Created)
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{courseId:guid}/lessons/order", async (Guid courseId, ReorderLessonsRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new ReorderLessonsCommand(courseId, request.LessonIds), cancellationToken)))
            .Produces<IReadOnlyList<LessonDto>>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{courseId:guid}/lessons/{lessonId:guid}", async (Guid courseId, Guid lessonId, LessonRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new UpdateLessonCommand(courseId, lessonId, request.LessonTitle), cancellationToken)))
            .Produces<LessonDto>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{courseId:guid}/lessons/{lessonId:guid}/preview", async (Guid courseId, Guid lessonId, LessonPreviewRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new SetLessonPreviewCommand(courseId, lessonId, request.IsPreview), cancellationToken)))
            .Produces<LessonDto>()
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapDelete("/{courseId:guid}/lessons/{lessonId:guid}", async (Guid courseId, Guid lessonId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteLessonCommand(courseId, lessonId), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        return api;
    }
}

/// <param name="InstructorProfileId">Id Hồ sơ Instructor chủ khóa; bắt buộc với Admin, Instructor bỏ trống hoặc truyền hồ sơ của mình.</param>
/// <param name="CategoryId">Id Category.</param>
/// <param name="Title">Tiêu đề khóa học (bắt buộc, tối đa 200 ký tự).</param>
/// <param name="Description">Mô tả khóa học (tùy chọn, tối đa 5000 ký tự).</param>
/// <param name="Price">Giá bằng đồng, số nguyên không âm (bắt buộc).</param>
public sealed record CreateCourseRequest(Guid? InstructorProfileId, Guid CategoryId, string? Title, string? Description, decimal? Price);

/// <param name="CategoryId">Id Category.</param>
/// <param name="Title">Tiêu đề khóa học (bắt buộc, tối đa 200 ký tự).</param>
/// <param name="Description">Mô tả khóa học (tùy chọn, tối đa 5000 ký tự).</param>
/// <param name="Price">Giá bằng đồng, số nguyên không âm (bắt buộc).</param>
public sealed record UpdateCourseRequest(Guid CategoryId, string? Title, string? Description, decimal? Price);

/// <param name="LessonTitle">Tiêu đề bài học (bắt buộc, tối đa 200 ký tự).</param>
public sealed record LessonRequest(string? LessonTitle);

/// <param name="LessonIds">Id mọi Bài học của khóa theo thứ tự mới, mỗi bài đúng một lần.</param>
public sealed record ReorderLessonsRequest(IReadOnlyList<Guid>? LessonIds);

/// <param name="IsPreview">true để đánh dấu Bài học xem thử, false để bỏ đánh dấu (bắt buộc).</param>
public sealed record LessonPreviewRequest(bool? IsPreview);

/// <param name="ContentType">Kiểu ảnh: <c>image/jpeg</c>, <c>image/png</c> hoặc <c>image/webp</c> (bắt buộc).</param>
/// <param name="ContentLength">Số byte của ảnh, lớn hơn 0 và tối đa 2 MB (bắt buộc).</param>
public sealed record ThumbnailUploadRequest(string? ContentType, long? ContentLength);

/// <param name="ObjectKey">Khóa ảnh do <c>thumbnail-upload</c> trả về, ảnh đã tải lên xong (bắt buộc).</param>
public sealed record SetThumbnailRequest(string? ObjectKey);
