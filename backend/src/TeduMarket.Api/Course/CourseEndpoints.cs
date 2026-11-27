using Mediator;
using TeduMarket.Api.Common;
using TeduMarket.Application.Course;
using TeduMarket.Application.Course.Commands.CreateCourse;
using TeduMarket.Application.Course.Commands.DeleteCourse;
using TeduMarket.Application.Course.Commands.UpdateCourse;

namespace TeduMarket.Api.Course;

/// <summary>Instructor và Admin tạo, sửa Khóa học ở trạng thái Chưa đăng (<c>/api/v1/courses</c>, FR-19, FR-22), Instructor/Admin xóa khóa; quyền sở hữu kiểm tra trong Handler.</summary>
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
