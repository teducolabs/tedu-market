using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.DeleteCourse;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Identity;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class DeleteCourseCommandHandlerTests
{
    private readonly FakeCategoryUnitOfWork _unitOfWork = new();
    private readonly InstructorProfileDto _ownerProfile = new(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
    private readonly CourseAggregate _course;

    public DeleteCourseCommandHandlerTests() =>
        _course = CourseAggregate.Create(_ownerProfile.InstructorProfileId, Guid.NewGuid(), "Xóa tôi", "xoa-toi", null, 1m, new FakeTimeProvider().GetUtcNow());

    private DeleteCourseCommandHandler CreateHandler(string role, FakeCourseRepository courses, InstructorProfileDto? byUserId = null) =>
        new(new FakeCategoryCurrentUser(role), new FakeCourseProfileReader(null, byUserId), courses, _unitOfWork);

    [Fact]
    public async Task Handle_OwnerInstructor_RemovesCourseAndSavesOnceAsync()
    {
        // Arrange
        var courses = new FakeCourseRepository(_course);
        var handler = CreateHandler(RoleNames.Instructor, courses, _ownerProfile);

        // Act
        await handler.Handle(new DeleteCourseCommand(_course.CourseId), CancellationToken.None);

        // Assert
        Assert.Same(_course, Assert.Single(courses.Removed));
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_AdminOnAnyCourse_RemovesCourseAsync()
    {
        // Arrange
        var courses = new FakeCourseRepository(_course);
        var handler = CreateHandler(RoleNames.Admin, courses);

        // Act
        await handler.Handle(new DeleteCourseCommand(_course.CourseId), CancellationToken.None);

        // Assert
        Assert.Same(_course, Assert.Single(courses.Removed));
    }

    [Fact]
    public async Task Handle_InstructorOfOtherCourse_ThrowsForbiddenWithoutRemovingAsync()
    {
        // Arrange
        var courses = new FakeCourseRepository(_course);
        var other = new InstructorProfileDto(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
        var handler = CreateHandler(RoleNames.Instructor, courses, other);

        // Act
        var act = async () => await handler.Handle(new DeleteCourseCommand(_course.CourseId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(courses.Removed);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_StudentRole_ThrowsForbiddenAsync()
    {
        // Arrange
        var courses = new FakeCourseRepository(_course);
        var handler = CreateHandler(RoleNames.Student, courses, _ownerProfile);

        // Act
        var act = async () => await handler.Handle(new DeleteCourseCommand(_course.CourseId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(courses.Removed);
    }

    [Fact]
    public async Task Handle_UnknownCourse_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Admin, new FakeCourseRepository());

        // Act
        var act = async () => await handler.Handle(new DeleteCourseCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
