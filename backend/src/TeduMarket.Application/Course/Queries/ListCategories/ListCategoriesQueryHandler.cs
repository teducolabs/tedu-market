using Mediator;

namespace TeduMarket.Application.Course.Queries.ListCategories;

public sealed class ListCategoriesQueryHandler(ICategoryReader categoryReader)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async ValueTask<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken) =>
        await categoryReader.ListAsync(cancellationToken);
}
