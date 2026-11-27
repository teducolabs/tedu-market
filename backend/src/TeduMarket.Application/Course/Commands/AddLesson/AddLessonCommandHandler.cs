using Mediator;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.AddLesson;

public sealed class AddLessonCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonRepository lessonRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<AddLessonCommand, LessonDto>
{
    public async ValueTask<LessonDto> Handle(AddLessonCommand command, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, command.CourseId, cancellationToken);
        var existing = await lessonRepository.ListByCourseAsync(command.CourseId, cancellationToken);
        var lesson = Lesson.Create(command.CourseId, command.LessonTitle, existing.Count + 1, timeProvider.GetUtcNow());
        lessonRepository.Add(lesson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return LessonAccess.ToDto(lesson);
    }
}
