using Mediator;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.DeleteLesson;

public sealed class DeleteLessonCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonRepository lessonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteLessonCommand>
{
    public async ValueTask<Unit> Handle(DeleteLessonCommand command, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, command.CourseId, cancellationToken);
        var lesson = await LessonAccess.FindLessonOfCourseAsync(lessonRepository, command.CourseId, command.LessonId, cancellationToken);
        lessonRepository.Remove(lesson);

        var remaining = (await lessonRepository.ListByCourseAsync(command.CourseId, cancellationToken))
            .Where(other => other.LessonId != lesson.LessonId);
        var lessonOrder = 1;
        foreach (var other in remaining)
        {
            other.MoveTo(lessonOrder++);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
