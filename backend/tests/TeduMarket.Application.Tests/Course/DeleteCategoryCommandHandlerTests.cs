using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.DeleteCategory;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly Category _category = Category.Create("Xóa tôi", new FakeTimeProvider().GetUtcNow());

    [Fact]
    public async Task Handle_Admin_RemovesCategoryAndSavesOnceAsync()
    {
        // Arrange
        var repository = new FakeCategoryRepository(_category);
        var handler = new DeleteCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Admin), repository, new FakeCourseRepository(), _unitOfWork);

        // Act
        await handler.Handle(new DeleteCategoryCommand(_category.CategoryId), CancellationToken.None);

        // Assert
        Assert.Same(_category, Assert.Single(repository.Removed));
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_Instructor_ThrowsForbiddenWithoutRemovingAsync()
    {
        // Arrange
        var repository = new FakeCategoryRepository(_category);
        var handler = new DeleteCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Instructor), repository, new FakeCourseRepository(), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteCategoryCommand(_category.CategoryId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(repository.Removed);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_UnknownCategory_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new DeleteCategoryCommandHandler(new FakeCategoryCurrentUser(RoleNames.Admin), new FakeCategoryRepository(), new FakeCourseRepository(), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_CategoryHasCourses_ThrowsCategoryInUseWithoutRemovingAsync()
    {
        // Arrange
        var repository = new FakeCategoryRepository(_category);
        var handler = new DeleteCategoryCommandHandler(
            new FakeCategoryCurrentUser(RoleNames.Admin), repository, new FakeCourseRepository(categoryInUse: true), _unitOfWork);

        // Act
        var act = async () => await handler.Handle(new DeleteCategoryCommand(_category.CategoryId), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<DomainException>(act);
        Assert.Equal("CATEGORY_IN_USE", exception.ErrorCode);
        Assert.Empty(repository.Removed);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
