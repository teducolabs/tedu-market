using Mediator;
using TeduMarket.Api.Common;
using TeduMarket.Application.Sample.Commands.CreateSample;
using TeduMarket.Application.Sample.Commands.SaveSampleNote;
using TeduMarket.Application.Sample.Queries.GetOwnedSample;
using TeduMarket.Application.Sample.Queries.GetSampleNotes;

namespace TeduMarket.Api.Sample;

/// <summary>Endpoint mẫu kiểm chứng khung API; chỉ map ở môi trường Development, xóa khi có endpoint nghiệp vụ thật.</summary>
public static class SampleEndpoints
{
    public static IEndpointRouteBuilder MapSampleEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/samples").WithTags("Sample");

        group.MapPost("/", async (CreateSampleCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(command, cancellationToken)));

        group.MapPost("/notes", async (SaveSampleNoteCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(command, cancellationToken)));

        group.MapGet("/notes", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetSampleNotesQuery(), cancellationToken)));

        group.MapGet("/cached", () => Results.Ok(new { value = Guid.NewGuid() }))
            .CacheOutput(ApiPolicies.CatalogCachePolicy);

        group.MapGet("/limited", () => Results.Ok())
            .RequireRateLimiting(ApiPolicies.CheckoutRateLimitPolicy);

        group.MapGet("/scheme", (HttpContext context) => Results.Ok(new { scheme = context.Request.Scheme }));

        group.MapGet("/admin-only", () => Results.Ok()).RequireAuthorization(AuthorizationPolicies.Admin);
        group.MapGet("/instructor-only", () => Results.Ok()).RequireAuthorization(AuthorizationPolicies.Instructor);
        group.MapGet("/student-only", () => Results.Ok()).RequireAuthorization(AuthorizationPolicies.Student);

        group.MapGet("/owned/{ownerId:guid}", async (Guid ownerId, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetOwnedSampleQuery(ownerId), cancellationToken)))
            .RequireAuthorization();

        group.MapGet("/failure", () =>
        {
            throw new InvalidOperationException("Lỗi mẫu không lường trước");
        });

        return api;
    }
}
