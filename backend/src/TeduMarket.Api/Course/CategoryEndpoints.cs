using Mediator;
using Microsoft.AspNetCore.OutputCaching;
using TeduMarket.Api.Common;
using TeduMarket.Application.Course;
using TeduMarket.Application.Course.Commands.CreateCategory;
using TeduMarket.Application.Course.Commands.DeleteCategory;
using TeduMarket.Application.Course.Commands.UpdateCategory;
using TeduMarket.Application.Course.Queries.ListCategories;

namespace TeduMarket.Api.Course;

/// <summary>Danh sách Category công khai và CRUD chỉ cho Admin (<c>/api/v1/categories</c>, FR-17); ghi xong thì xóa cache <c>catalog</c> để danh sách công khai cập nhật ngay.</summary>
public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
                Results.Ok(await mediator.Send(new ListCategoriesQuery(), cancellationToken)))
            .Produces<IReadOnlyList<CategoryDto>>()
            .AllowAnonymous()
            .CacheOutput(ApiPolicies.CatalogCachePolicy);

        group.MapPost("/", async (SaveCategoryRequest request, IMediator mediator, IOutputCacheStore cacheStore, CancellationToken cancellationToken) =>
            {
                var category = await mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken);
                await cacheStore.EvictByTagAsync(ApiPolicies.CatalogCacheTag, cancellationToken);
                return Results.Created($"/api/v1/categories/{category.CategoryId}", category);
            })
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapPut("/{categoryId:guid}", async (Guid categoryId, SaveCategoryRequest request, IMediator mediator, IOutputCacheStore cacheStore, CancellationToken cancellationToken) =>
            {
                var category = await mediator.Send(new UpdateCategoryCommand(categoryId, request.Name), cancellationToken);
                await cacheStore.EvictByTagAsync(ApiPolicies.CatalogCacheTag, cancellationToken);
                return Results.Ok(category);
            })
            .Produces<CategoryDto>()
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapDelete("/{categoryId:guid}", async (Guid categoryId, IMediator mediator, IOutputCacheStore cacheStore, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
                await cacheStore.EvictByTagAsync(ApiPolicies.CatalogCacheTag, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthorizationPolicies.Admin);

        return api;
    }
}

/// <param name="Name">Tên Category (bắt buộc, tối đa 100 ký tự).</param>
public sealed record SaveCategoryRequest(string? Name);
