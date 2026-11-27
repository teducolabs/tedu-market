using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course;
using TeduMarket.Application.Course.Queries.ListLessons;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class ListLessonsQueryHandlerTests
{
    private readonly LessonScenario _scenario = new();

    private ListLessonsQueryHandler CreateHandler(string role, bool owner, IReadOnlyList<LessonDto> lessons) =>
        new(new FakeCategoryCurrentUser(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, new FakeLessonReader(lessons));

    [Fact]
    public async Task Handle_OwnerInstructor_ReturnsLessonsFromReaderAsync()
    {
        // Arrange
        IReadOnlyList<LessonDto> lessons = [new(Guid.NewGuid(), _scenario.Course.CourseId, "Bài 1", 1, true)];
        var handler = CreateHandler(RoleNames.Instructor, owner: true, lessons);

        // Act
        var result = await handler.Handle(new ListLessonsQuery(_scenario.Course.CourseId), CancellationToken.None);

        // Assert
        Assert.Same(lessons, result);
    }

    [Fact]
    public async Task Handle_InstructorOfOtherCourse_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = CreateHandler(RoleNames.Instructor, owner: false, []);

        // Act
        var act = async () => await handler.Handle(new ListLessonsQuery(_scenario.Course.CourseId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }
}
