using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.CreateCategory;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class CreateCategoryCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 27, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_Admin_AddsCategoryAndSavesOnceAsync()
    {
        // Arrange
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Admin), repository, _unitOfWork, _clock);

        // Act
        var result = await handler.Handle(new CreateCategoryCommand(" Lập trình "), CancellationToken.None);

        // Assert
        var added = Assert.Single(repository.Added);
        Assert.Equal("Lập trình", added.Name);
        Assert.Equal(added.CategoryId, result.CategoryId);
        Assert.Equal("Lập trình", result.Name);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(RoleNames.Instructor)]
    [InlineData(RoleNames.Student)]
    public async Task Handle_NotAdmin_ThrowsForbiddenWithoutChangesAsync(string role)
    {
        // Arrange
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryCommandHandler(new FakeCategoryCurrentUser(role), repository, _unitOfWork, _clock);

        // Act
        var act = async () => await handler.Handle(new CreateCategoryCommand("A"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(repository.Added);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_NameTaken_ThrowsFieldErrorOnNameWithoutChangesAsync()
    {
        // Arrange
        var repository = new FakeCategoryRepository(nameTaken: true);
        var handler = new CreateCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Admin), repository, _unitOfWork, _clock);

        // Act
        var act = async () => await handler.Handle(new CreateCategoryCommand("Trùng"), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("name"));
        Assert.Empty(repository.Added);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
