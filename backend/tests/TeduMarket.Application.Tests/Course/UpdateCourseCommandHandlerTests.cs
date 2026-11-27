using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.UpdateCourse;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class UpdateCourseCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly Category _category = Category.Create("Lập trình", new FakeTimeProvider().GetUtcNow());
    private readonly InstructorProfileDto _ownerProfile = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
    private readonly CourseAggregate _course;

    public UpdateCourseCommandHandlerTests() =>
        _course = CourseAggregate.Create(_ownerProfile.InstructorProfileId, _category.CategoryId, "Cũ", "cu", null, 1m, new FakeTimeProvider().GetUtcNow());

    private UpdateCourseCommandHandler CreateHandler(string role, InstructorProfileDto? byUserId = null, CourseAggregate? course = null, bool categoryExists = true) =>
        new(
            new FakeCategoryCurrentUser(role),
            new FakeCourseProfileReader(null, byUserId),
            new FakeCategoryRepository(categoryExists ? _category : null),
            new FakeCourseRepository(course),
            _unitOfWork,
            new FakeThumbnailStorage(),
            new FakeTimeProvider());

    [Fact]
    public async Task Handle_OwnerInstructor_UpdatesCourseAndSavesOnceAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, _ownerProfile, _course);

        // Act
        var dto = await handler.Handle(new UpdateCourseCommand(_course.CourseId, _category.CategoryId, " Mới ", "Mô tả", 250_000m), CancellationToken.None);

        // Assert
        Assert.Equal("Mới", _course.Title);
        Assert.Equal(250_000m, dto.Price);
        Assert.Equal("Draft", dto.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorOfOtherCourse_ThrowsForbiddenAndKeepsCourseAsync()
    {
        // Arrange
        var other = new InstructorProfileDto(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
        var handler = CreateHandler(RoleNames.Instructor, other, _course);

        // Act
        var act = async () => await handler.Handle(new UpdateCourseCommand(_course.CourseId, _category.CategoryId, "Chiếm", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal("Cũ", _course.Title);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorWithoutProfile_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, null, _course);

        // Act
        var act = async () => await handler.Handle(new UpdateCourseCommand(_course.CourseId, _category.CategoryId, "Chiếm", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal("Cũ", _course.Title);
    }

    [Fact]
    public async Task Handle_AdminOnAnyCourse_UpdatesCourseAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin, null, _course);

        // Act
        await handler.Handle(new UpdateCourseCommand(_course.CourseId, _category.CategoryId, "Admin sửa", null, 0m), CancellationToken.None);

        // Assert
        Assert.Equal("Admin sửa", _course.Title);
        Assert.Equal(_ownerProfile.InstructorProfileId, _course.InstructorProfileId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_StudentRole_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Student, _ownerProfile, _course);

        // Act
        var act = async () => await handler.Handle(new UpdateCourseCommand(_course.CourseId, _category.CategoryId, "X", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal("Cũ", _course.Title);
    }

    [Fact]
    public async Task Handle_UnknownCourse_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new UpdateCourseCommand(Guid.NewGuid(), _category.CategoryId, "X", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_UnknownCategory_ThrowsFieldErrorOnCategoryIdAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin, null, _course, categoryExists: false);

        // Act
        var act = async () => await handler.Handle(new UpdateCourseCommand(_course.CourseId, Guid.NewGuid(), "X", null, 0m), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("categoryId"));
        Assert.Equal("Cũ", _course.Title);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
