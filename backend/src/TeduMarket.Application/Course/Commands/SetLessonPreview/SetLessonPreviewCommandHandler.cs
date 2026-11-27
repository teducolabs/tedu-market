using Mediator;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.SetLessonPreview;

public sealed class SetLessonPreviewCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonRepository lessonRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<SetLessonPreviewCommand, LessonDto>
{
    public async ValueTask<LessonDto> Handle(SetLessonPreviewCommand command, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, command.CourseId, cancellationToken);
        var lesson = await LessonAccess.FindLessonOfCourseAsync(lessonRepository, command.CourseId, command.LessonId, cancellationToken);
        lesson.SetPreview(command.IsPreview!.Value, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return LessonAccess.ToDto(lesson);
    }
}
