using Mediator;
using TeduMarket.Api.Common;
using TeduMarket.Application.Instructor.Commands.DeleteInstructorProfile;
using TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;
using TeduMarket.Application.Instructor.Queries.GetInstructorProfile;
using TeduMarket.Application.Instructor.Queries.GetMyInstructorProfile;

namespace TeduMarket.Api.Instructor;

/// <summary>Đọc, sửa, xóa Hồ sơ Instructor (<c>/api/v1/instructor-profiles</c>); kiểm tra chủ hồ sơ làm trong Handler.</summary>
public static class InstructorProfileEndpoints
{
    public static IEndpointRouteBuilder MapInstructorProfileEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/instructor-profiles").WithTags("InstructorProfiles");

        group.MapGet("/me", async (IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new GetMyInstructorProfileQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.Instructor);

        group.MapGet("/{instructorProfileId:guid}", async (Guid instructorProfileId, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new GetInstructorProfileQuery(instructorProfileId), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapPut("/{instructorProfileId:guid}", async (Guid instructorProfileId, UpdateInstructorProfileRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(
                    new UpdateInstructorProfileCommand(instructorProfileId, request.DisplayName, request.Biography, request.AvatarUrl), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.InstructorOrAdmin);

        group.MapDelete("/{instructorProfileId:guid}", async (Guid instructorProfileId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteInstructorProfileCommand(instructorProfileId), cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Admin);

        return api;
    }
}

/// <param name="DisplayName">Tên hiển thị (bắt buộc).</param>
/// <param name="Biography">Giới thiệu (tùy chọn).</param>
/// <param name="AvatarUrl">Url ảnh đại diện http/https (tùy chọn).</param>
public sealed record UpdateInstructorProfileRequest(string? DisplayName, string? Biography, string? AvatarUrl);
