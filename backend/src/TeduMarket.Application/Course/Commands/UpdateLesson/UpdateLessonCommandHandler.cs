using Mediator;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.UpdateLesson;

public sealed class UpdateLessonCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonRepository lessonRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateLessonCommand, LessonDto>
{
    public async ValueTask<LessonDto> Handle(UpdateLessonCommand command, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, command.CourseId, cancellationToken);
        var lesson = await LessonAccess.FindLessonOfCourseAsync(lessonRepository, command.CourseId, command.LessonId, cancellationToken);
        lesson.Rename(command.LessonTitle, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return LessonAccess.ToDto(lesson);
    }
}
