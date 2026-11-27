using Microsoft.Extensions.Time.Testing;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Course.Commands.AddLesson;
using TeduMarket.Application.Course.Commands.DeleteLesson;
using TeduMarket.Application.Course.Commands.ReorderLessons;
using TeduMarket.Application.Course.Commands.SetLessonPreview;
using TeduMarket.Application.Course.Commands.UpdateLesson;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class LessonCommandHandlerTests
{
    private readonly LessonScenario _scenario = new();
    private readonly FakeTimeProvider _time = new();

    private static FakeCategoryCurrentUser UserOf(string role) => new(role);

    private AddLessonCommandHandler AddHandler(string role, bool owner = true) =>
        new(UserOf(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, _scenario.LessonRepository, _scenario.UnitOfWork, _time);

    private UpdateLessonCommandHandler UpdateHandler(string role, bool owner = true) =>
        new(UserOf(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, _scenario.LessonRepository, _scenario.UnitOfWork, _time);

    private DeleteLessonCommandHandler DeleteHandler(string role, bool owner = true) =>
        new(UserOf(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, _scenario.LessonRepository, _scenario.UnitOfWork);

    private ReorderLessonsCommandHandler ReorderHandler(string role, bool owner = true) =>
        new(UserOf(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, _scenario.LessonRepository, _scenario.UnitOfWork);

    private SetLessonPreviewCommandHandler PreviewHandler(string role, bool owner = true) =>
        new(UserOf(role), _scenario.ProfileReaderFor(owner), _scenario.Courses, _scenario.LessonRepository, _scenario.UnitOfWork, _time);

    [Fact]
    public async Task Add_OwnerInstructor_AppendsLessonAtEndAndSavesOnceAsync()
    {
        // Arrange
        var handler = AddHandler(RoleNames.Instructor);

        // Act
        var dto = await handler.Handle(new AddLessonCommand(_scenario.Course.CourseId, " Bài mới "), CancellationToken.None);

        // Assert
        Assert.Equal(4, dto.LessonOrder);
        Assert.Equal("Bài mới", dto.LessonTitle);
        Assert.False(dto.IsPreview);
        Assert.Equal(_scenario.Course.CourseId, dto.CourseId);
        Assert.Single(_scenario.LessonRepository.Added);
        Assert.Equal(1, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Add_AdminOnAnyCourse_AppendsLessonAsync()
    {
        // Arrange
        var handler = AddHandler(RoleNames.Admin, owner: false);

        // Act
        var dto = await handler.Handle(new AddLessonCommand(_scenario.Course.CourseId, "Admin thêm"), CancellationToken.None);

        // Assert
        Assert.Equal(4, dto.LessonOrder);
    }

    [Fact]
    public async Task Add_InstructorOfOtherCourse_ThrowsForbiddenWithoutSavingAsync()
    {
        // Arrange
        var handler = AddHandler(RoleNames.Instructor, owner: false);

        // Act
        var act = async () => await handler.Handle(new AddLessonCommand(_scenario.Course.CourseId, "Xâm nhập"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(_scenario.LessonRepository.Added);
        Assert.Equal(0, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Add_StudentRole_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = AddHandler(RoleNames.Student);

        // Act
        var act = async () => await handler.Handle(new AddLessonCommand(_scenario.Course.CourseId, "A"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task Add_UnknownCourse_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = new AddLessonCommandHandler(
            UserOf(RoleNames.Admin), _scenario.ProfileReaderFor(true), new FakeCourseRepository(), _scenario.LessonRepository, _scenario.UnitOfWork, _time);

        // Act
        var act = async () => await handler.Handle(new AddLessonCommand(Guid.NewGuid(), "A"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Add_BlankTitle_ThrowsDomainExceptionWithoutSavingAsync()
    {
        // Arrange
        var handler = AddHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new AddLessonCommand(_scenario.Course.CourseId, " "), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);
        Assert.Equal(0, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_OwnerInstructor_RenamesLessonAndKeepsOrderAsync()
    {
        // Arrange
        var lesson = _scenario.Lessons[1];
        var handler = UpdateHandler(RoleNames.Instructor);

        // Act
        var dto = await handler.Handle(new UpdateLessonCommand(_scenario.Course.CourseId, lesson.LessonId, "Đổi tên"), CancellationToken.None);

        // Assert
        Assert.Equal("Đổi tên", dto.LessonTitle);
        Assert.Equal(2, dto.LessonOrder);
        Assert.Equal(1, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_InstructorOfOtherCourse_ThrowsForbiddenAndKeepsTitleAsync()
    {
        // Arrange
        var lesson = _scenario.Lessons[0];
        var handler = UpdateHandler(RoleNames.Instructor, owner: false);

        // Act
        var act = async () => await handler.Handle(new UpdateLessonCommand(_scenario.Course.CourseId, lesson.LessonId, "Chiếm"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal("Bài 1", lesson.LessonTitle);
        Assert.Equal(0, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_LessonOfAnotherCourse_ThrowsNotFoundAndKeepsTitleAsync()
    {
        // Arrange
        var foreign = TeduMarket.Domain.Course.Lesson.Create(Guid.NewGuid(), "Của khóa khác", 1, _scenario.Now);
        var repository = new FakeLessonRepository([foreign]);
        var handler = new UpdateLessonCommandHandler(
            UserOf(RoleNames.Admin), _scenario.ProfileReaderFor(true), _scenario.Courses, repository, _scenario.UnitOfWork, _time);

        // Act
        var act = async () => await handler.Handle(new UpdateLessonCommand(_scenario.Course.CourseId, foreign.LessonId, "Chiếm"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("Của khóa khác", foreign.LessonTitle);
    }

    [Fact]
    public async Task Delete_MiddleLesson_RemovesItAndCompactsOrderAsync()
    {
        // Arrange
        var handler = DeleteHandler(RoleNames.Instructor);

        // Act
        await handler.Handle(new DeleteLessonCommand(_scenario.Course.CourseId, _scenario.Lessons[1].LessonId), CancellationToken.None);

        // Assert
        Assert.Same(_scenario.Lessons[1], Assert.Single(_scenario.LessonRepository.Removed));
        Assert.Equal(1, _scenario.Lessons[0].LessonOrder);
        Assert.Equal(2, _scenario.Lessons[2].LessonOrder);
        Assert.Equal(1, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_InstructorOfOtherCourse_ThrowsForbiddenWithoutRemovingAsync()
    {
        // Arrange
        var handler = DeleteHandler(RoleNames.Instructor, owner: false);

        // Act
        var act = async () => await handler.Handle(new DeleteLessonCommand(_scenario.Course.CourseId, _scenario.Lessons[0].LessonId), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Empty(_scenario.LessonRepository.Removed);
    }

    [Fact]
    public async Task Delete_UnknownLesson_ThrowsNotFoundAsync()
    {
        // Arrange
        var handler = DeleteHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new DeleteLessonCommand(_scenario.Course.CourseId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task Reorder_FullPermutation_StoresAndReturnsNewOrderAsync()
    {
        // Arrange
        var ids = new[] { _scenario.Lessons[2].LessonId, _scenario.Lessons[0].LessonId, _scenario.Lessons[1].LessonId };
        var handler = ReorderHandler(RoleNames.Instructor);

        // Act
        var result = await handler.Handle(new ReorderLessonsCommand(_scenario.Course.CourseId, ids), CancellationToken.None);

        // Assert
        Assert.Equal(ids, result.Select(lesson => lesson.LessonId));
        Assert.Equal([1, 2, 3], result.Select(lesson => lesson.LessonOrder));
        Assert.Equal(1, _scenario.Lessons[2].LessonOrder);
        Assert.Equal(1, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Reorder_MissingLesson_ThrowsFieldErrorWithoutChangingOrderAsync()
    {
        // Arrange
        var ids = new[] { _scenario.Lessons[1].LessonId, _scenario.Lessons[0].LessonId };
        var handler = ReorderHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new ReorderLessonsCommand(_scenario.Course.CourseId, ids), CancellationToken.None);

        // Assert
        var error = await Assert.ThrowsAsync<RequestValidationException>(act);
        Assert.True(error.Errors.ContainsKey("lessonIds"));
        Assert.Equal(1, _scenario.Lessons[0].LessonOrder);
        Assert.Equal(0, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Reorder_DuplicateAndForeignId_ThrowsFieldErrorAsync()
    {
        // Arrange
        var ids = new[] { _scenario.Lessons[0].LessonId, _scenario.Lessons[0].LessonId, Guid.NewGuid() };
        var handler = ReorderHandler(RoleNames.Admin);

        // Act
        var act = async () => await handler.Handle(new ReorderLessonsCommand(_scenario.Course.CourseId, ids), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<RequestValidationException>(act);
    }

    [Fact]
    public async Task Reorder_InstructorOfOtherCourse_ThrowsForbiddenAndKeepsOrderAsync()
    {
        // Arrange
        var ids = new[] { _scenario.Lessons[2].LessonId, _scenario.Lessons[1].LessonId, _scenario.Lessons[0].LessonId };
        var handler = ReorderHandler(RoleNames.Instructor, owner: false);

        // Act
        var act = async () => await handler.Handle(new ReorderLessonsCommand(_scenario.Course.CourseId, ids), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Equal(1, _scenario.Lessons[0].LessonOrder);
    }

    [Fact]
    public async Task SetPreview_OwnerInstructorMarksLesson_ReturnsPreviewFlagAsync()
    {
        // Arrange
        var handler = PreviewHandler(RoleNames.Instructor);

        // Act
        var dto = await handler.Handle(new SetLessonPreviewCommand(_scenario.Course.CourseId, _scenario.Lessons[0].LessonId, true), CancellationToken.None);

        // Assert
        Assert.True(dto.IsPreview);
        Assert.True(_scenario.Lessons[0].IsPreview);
        Assert.Equal(1, _scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task SetPreview_AdminUnmarksLesson_ClearsFlagAsync()
    {
        // Arrange
        _scenario.Lessons[0].SetPreview(true, _scenario.Now);
        var handler = PreviewHandler(RoleNames.Admin, owner: false);

        // Act
        var dto = await handler.Handle(new SetLessonPreviewCommand(_scenario.Course.CourseId, _scenario.Lessons[0].LessonId, false), CancellationToken.None);

        // Assert
        Assert.False(dto.IsPreview);
    }

    [Fact]
    public async Task SetPreview_InstructorOfOtherCourse_ThrowsForbiddenAndKeepsFlagAsync()
    {
        // Arrange
        var handler = PreviewHandler(RoleNames.Instructor, owner: false);

        // Act
        var act = async () => await handler.Handle(
            new SetLessonPreviewCommand(_scenario.Course.CourseId, _scenario.Lessons[0].LessonId, true), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.False(_scenario.Lessons[0].IsPreview);
        Assert.Equal(0, _scenario.UnitOfWork.SaveCount);
    }
}
