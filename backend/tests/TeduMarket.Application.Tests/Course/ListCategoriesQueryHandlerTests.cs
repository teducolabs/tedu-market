using TeduMarket.Application.Course;
using TeduMarket.Application.Course.Queries.ListCategories;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class ListCategoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Categories_ReturnsWhatReaderReturnsAsync()
    {
        // Arrange
        CategoryDto[] categories = [new(Guid.NewGuid(), "A"), new(Guid.NewGuid(), "B")];
        var handler = new ListCategoriesQueryHandler(new FakeCategoryReader(categories));

        // Act
        var result = await handler.Handle(new ListCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(categories, result);
    }
}
