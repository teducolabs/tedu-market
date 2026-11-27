using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.UpdateCategory;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class UpdateCategoryCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 27, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_Admin_RenamesCategoryExcludingItselfFromNameCheckAsync()
    {
        // Arrange
        var category = Category.Create("Cũ", _clock.GetUtcNow());
        var repository = new FakeCategoryRepository(category);
        var handler = new UpdateCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Admin), repository, _unitOfWork, _clock);

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(category.CategoryId, " Mới "), CancellationToken.None);

        // Assert
        Assert.Equal("Mới", category.Name);
        Assert.Equal("Mới", result.Name);
        Assert.Equal(category.CategoryId, repository.ExcludedIdSeen);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_Instructor_ThrowsForbiddenAndKeepsNameAsync()
    {
        // Arrange
        var category = Category.Create("Cũ", _clock.GetUtcNow());
        var handler = new UpdateCategoryCommandHandler(
            new FakeCategoryCurrentUser(RoleNames.Instructor), new FakeCategoryRepository(category), _unitOfWork, _clock);

        // Act
        var act = async () => await handler.Handle(new UpdateCategoryCommand(category.CategoryId, "Mới"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal("Cũ", category.Name);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_UnknownCategory_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new UpdateCategoryCommandHandler(
            new FakeCategoryCurrentUser(RoleNames.Admin), new FakeCategoryRepository(), _unitOfWork, _clock);

        // Act
        var act = async () => await handler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Mới"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_NameTaken_ThrowsFieldErrorOnNameAndKeepsNameAsync()
    {
        // Arrange
        var category = Category.Create("Cũ", _clock.GetUtcNow());
        var handler = new UpdateCategoryCommandHandler(
            new FakeCategoryCurrentUser(RoleNames.Admin), new FakeCategoryRepository(category, nameTaken: true), _unitOfWork, _clock);

        // Act
        var act = async () => await handler.Handle(new UpdateCategoryCommand(category.CategoryId, "Trùng"), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("name"));
        Assert.Equal("Cũ", category.Name);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
