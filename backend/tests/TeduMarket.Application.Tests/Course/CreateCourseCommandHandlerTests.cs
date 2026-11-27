using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.CreateCourse;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class CreateCourseCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly FakeCourseRepository _courses = new();
    private readonly Category _category = Category.Create("Lập trình", new FakeTimeProvider().GetUtcNow());
    private readonly InstructorProfileDto _ownProfile = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);

    private CreateCourseCommandHandler CreateHandler(string role, InstructorProfileDto? byId = null, InstructorProfileDto? byUserId = null, bool categoryExists = true) =>
        new(
            new FakeCategoryCurrentUser(role),
            new FakeCourseProfileReader(byId, byUserId),
            new FakeCategoryRepository(categoryExists ? _category : null),
            _courses,
            _unitOfWork,
            new FakeTimeProvider());

    [Fact]
    public async Task Handle_InstructorOmitsProfileId_CreatesDraftOwnedByOwnProfileAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, byUserId: _ownProfile);

        // Act
        var dto = await handler.Handle(new CreateCourseCommand(null, _category.CategoryId, " Khóa A ", "Mô tả", 100_000m), CancellationToken.None);

        // Assert
        var saved = Assert.Single(_courses.Added);
        Assert.Equal(_ownProfile.InstructorProfileId, saved.InstructorProfileId);
        Assert.Equal(CourseStatus.Draft, saved.Status);
        Assert.Equal("Draft", dto.Status);
        Assert.Equal("Khóa A", dto.Title);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorPassesOwnProfileId_CreatesCourseAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, byUserId: _ownProfile);

        // Act
        await handler.Handle(new CreateCourseCommand(_ownProfile.InstructorProfileId, _category.CategoryId, "A", null, 0m), CancellationToken.None);

        // Assert
        Assert.Single(_courses.Added);
    }

    [Fact]
    public async Task Handle_InstructorPassesOtherProfileId_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, byUserId: _ownProfile);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(Guid.NewGuid(), _category.CategoryId, "A", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(_courses.Added);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_InstructorWithoutProfile_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(null, _category.CategoryId, "A", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(_courses.Added);
    }

    [Fact]
    public async Task Handle_StudentRole_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Student, byUserId: _ownProfile);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(null, _category.CategoryId, "A", null, 0m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(_courses.Added);
    }

    [Fact]
    public async Task Handle_AdminWithExistingProfileId_CreatesCourseForThatInstructorAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin, byId: _ownProfile);

        // Act
        await handler.Handle(new CreateCourseCommand(_ownProfile.InstructorProfileId, _category.CategoryId, "A", null, 5m), CancellationToken.None);

        // Assert
        Assert.Equal(_ownProfile.InstructorProfileId, Assert.Single(_courses.Added).InstructorProfileId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_AdminWithoutProfileId_ThrowsFieldErrorOnInstructorProfileIdAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin, byId: _ownProfile);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(null, _category.CategoryId, "A", null, 5m), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("instructorProfileId"));
        Assert.Empty(_courses.Added);
    }

    [Fact]
    public async Task Handle_AdminWithUnknownProfileId_ThrowsFieldErrorOnInstructorProfileIdAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(Guid.NewGuid(), _category.CategoryId, "A", null, 5m), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("instructorProfileId"));
    }

    [Fact]
    public async Task Handle_UnknownCategory_ThrowsFieldErrorOnCategoryIdAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, byUserId: _ownProfile, categoryExists: false);

        // Act
        var act = async () => await handler.Handle(new CreateCourseCommand(null, Guid.NewGuid(), "A", null, 0m), CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(exception.Errors.ContainsKey("categoryId"));
        Assert.Empty(_courses.Added);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_TitleAndSlugAlreadyTaken_SavesCourseWithNextSuffixedSlugAsync()
    {
        // Arrange
        var courses = new FakeCourseRepository(slugs: new HashSet<string> { "khoa-a", "khoa-a-2" });
        var handler = new CreateCourseCommandHandler(
            new FakeCategoryCurrentUser(RoleNames.Instructor),
            new FakeCourseProfileReader(null, _ownProfile),
            new FakeCategoryRepository(_category),
            courses,
            _unitOfWork,
            new FakeTimeProvider());

        // Act
        var dto = await handler.Handle(new CreateCourseCommand(null, _category.CategoryId, "Khóa A", null, 0m), CancellationToken.None);

        // Assert
        Assert.Equal("khoa-a-3", dto.Slug);
        Assert.Equal("khoa-a", courses.PrefixSeen);
        Assert.Equal("khoa-a-3", Assert.Single(courses.Added).Slug);
    }
}
