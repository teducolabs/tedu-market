using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.CreateThumbnailUpload;
using TeduMarket.Application.Course.Commands.SetCourseThumbnail;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Identity;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class ThumbnailCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly InstructorProfileDto _ownerProfile = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
    private readonly CourseAggregate _course;

    public ThumbnailCommandHandlerTests() =>
        _course = CourseAggregate.Create(_ownerProfile.InstructorProfileId, Guid.NewGuid(), "Khóa", "khoa", null, 1m, new FakeTimeProvider().GetUtcNow());

    private static CreateThumbnailUploadCommandHandler CreateUploadHandler(string role, InstructorProfileDto? byUserId, FakeThumbnailStorage storage, CourseAggregate? course) =>
        new(new FakeCategoryCurrentUser(role), new FakeCourseProfileReader(null, byUserId), new FakeCourseRepository(course), storage);

    private SetCourseThumbnailCommandHandler CreateSetHandler(string role, InstructorProfileDto? byUserId, FakeThumbnailStorage storage, CourseAggregate? course) =>
        new(new FakeCategoryCurrentUser(role), new FakeCourseProfileReader(null, byUserId), new FakeCourseRepository(course), _unitOfWork, storage, new FakeTimeProvider());

    private string NewKey() => $"{_course.CourseId:N}/{Guid.NewGuid():N}.png";

    [Fact]
    public async Task CreateUpload_OwnerInstructor_ReturnsTicketForKeyUnderCourseAsync()
    {
        // Arrange
        var storage = new FakeThumbnailStorage();
        var handler = CreateUploadHandler(RoleNames.Instructor, _ownerProfile, storage, _course);

        // Act
        var dto = await handler.Handle(new CreateThumbnailUploadCommand(_course.CourseId, "IMAGE/PNG", 5000), CancellationToken.None);

        // Assert
        Assert.StartsWith($"{_course.CourseId:N}/", dto.ObjectKey, StringComparison.Ordinal);
        Assert.EndsWith(".png", dto.ObjectKey, StringComparison.Ordinal);
        Assert.Equal("image/png", dto.ContentType);
        Assert.Equal(5000, dto.ContentLength);
        Assert.Equal((dto.ObjectKey, "image/png", 5000L), Assert.Single(storage.Uploads));
        Assert.Contains(dto.ObjectKey, dto.UploadUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateUpload_Admin_ReturnsTicketForAnyCourseAsync()
    {
        // Arrange
        var handler = CreateUploadHandler(RoleNames.Admin, null, new FakeThumbnailStorage(), _course);

        // Act
        var dto = await handler.Handle(new CreateThumbnailUploadCommand(_course.CourseId, "image/webp", 10), CancellationToken.None);

        // Assert
        Assert.EndsWith(".webp", dto.ObjectKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateUpload_InstructorOfOtherCourse_ThrowsForbiddenAndIssuesNoUrlAsync()
    {
        // Arrange
        var other = new InstructorProfileDto(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
        var storage = new FakeThumbnailStorage();
        var handler = CreateUploadHandler(RoleNames.Instructor, other, storage, _course);

        // Act
        var act = async () => await handler.Handle(new CreateThumbnailUploadCommand(_course.CourseId, "image/png", 10), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(storage.Uploads);
    }

    [Fact]
    public async Task CreateUpload_StudentRole_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateUploadHandler(RoleNames.Student, null, new FakeThumbnailStorage(), _course);

        // Act
        var act = async () => await handler.Handle(new CreateThumbnailUploadCommand(_course.CourseId, "image/png", 10), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task CreateUpload_CourseMissing_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = CreateUploadHandler(RoleNames.Admin, null, new FakeThumbnailStorage(), null);

        // Act
        var act = async () => await handler.Handle(new CreateThumbnailUploadCommand(Guid.NewGuid(), "image/png", 10), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task SetThumbnail_OwnerWithUploadedFile_SavesKeyOnceAndReturnsUrlAsync()
    {
        // Arrange
        var key = NewKey();
        var handler = CreateSetHandler(RoleNames.Instructor, _ownerProfile, new FakeThumbnailStorage(), _course);

        // Act
        var dto = await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, key), CancellationToken.None);

        // Assert
        Assert.Equal(key, _course.ThumbnailKey);
        Assert.Equal($"https://storage.test/thumbnails/{key}", dto.ThumbnailUrl);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetThumbnail_ReplacingExistingThumbnail_DeletesPreviousObjectAfterSavingAsync()
    {
        // Arrange
        var oldKey = NewKey();
        _course.SetThumbnail(oldKey, new FakeTimeProvider().GetUtcNow());
        var storage = new FakeThumbnailStorage();
        var handler = CreateSetHandler(RoleNames.Instructor, _ownerProfile, storage, _course);
        var newKey = NewKey();

        // Act
        await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, newKey), CancellationToken.None);

        // Assert
        Assert.Equal(newKey, _course.ThumbnailKey);
        Assert.Equal([oldKey], storage.Deleted);
    }

    [Fact]
    public async Task SetThumbnail_FirstThumbnail_DeletesNothingAsync()
    {
        // Arrange
        var storage = new FakeThumbnailStorage();
        var handler = CreateSetHandler(RoleNames.Instructor, _ownerProfile, storage, _course);

        // Act
        await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, NewKey()), CancellationToken.None);

        // Assert
        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task SetThumbnail_SameKeyAgain_KeepsObjectAsync()
    {
        // Arrange
        var key = NewKey();
        _course.SetThumbnail(key, new FakeTimeProvider().GetUtcNow());
        var storage = new FakeThumbnailStorage();
        var handler = CreateSetHandler(RoleNames.Instructor, _ownerProfile, storage, _course);

        // Act
        await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, key), CancellationToken.None);

        // Assert
        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task SetThumbnail_FileNotUploaded_ThrowsFieldErrorAndKeepsCourseAsync()
    {
        // Arrange
        var handler = CreateSetHandler(RoleNames.Admin, null, new FakeThumbnailStorage(exists: false), _course);

        // Act
        var act = async () => await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, NewKey()), CancellationToken.None);

        // Assert
        var error = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.Contains("objectKey", error.Errors.Keys);
        Assert.Null(_course.ThumbnailKey);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetThumbnail_InstructorOfOtherCourse_ThrowsForbiddenAndKeepsCourseAsync()
    {
        // Arrange
        var other = new InstructorProfileDto(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
        var handler = CreateSetHandler(RoleNames.Instructor, other, new FakeThumbnailStorage(), _course);

        // Act
        var act = async () => await handler.Handle(new SetCourseThumbnailCommand(_course.CourseId, NewKey()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Null(_course.ThumbnailKey);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetThumbnail_CourseMissing_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = CreateSetHandler(RoleNames.Admin, null, new FakeThumbnailStorage(), null);

        // Act
        var act = async () => await handler.Handle(new SetCourseThumbnailCommand(Guid.NewGuid(), NewKey()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}
