using Mediator;
using TeduMarket.Api.Common;
using TeduMarket.Application.Common.Paging;
using TeduMarket.Application.Identity.Queries.ListUsers;
using TeduMarket.Application.Instructor.Commands.ApproveInstructor;

namespace TeduMarket.Api.Instructor;

/// <summary>Admin xem danh sách người dùng (<c>GET /api/v1/users</c>) và Duyệt Instructor (<c>POST /api/v1/users/{userId}/instructor-approval</c>).</summary>
public static class InstructorApprovalEndpoints
{
    public static IEndpointRouteBuilder MapInstructorApprovalEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users").RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet("/", async (string? role, int? pageNumber, int? pageSize, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(
                new ListUsersQuery(role, pageNumber ?? 1, pageSize ?? ListUsersQuery.DefaultPageSize), cancellationToken)))
            .Produces<PagedResult<UserDto>>();

        group.MapPost("/{userId:guid}/instructor-approval", async (Guid userId, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new ApproveInstructorCommand(userId), cancellationToken)))
            .Produces<InstructorApprovalDto>();

        return api;
    }
}
