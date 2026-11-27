using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.ReorderLessons;

public sealed class ReorderLessonsCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonRepository lessonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderLessonsCommand, IReadOnlyList<LessonDto>>
{
    public async ValueTask<IReadOnlyList<LessonDto>> Handle(ReorderLessonsCommand command, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, command.CourseId, cancellationToken);
        var lessons = await lessonRepository.ListByCourseAsync(command.CourseId, cancellationToken);
        var requestedIds = command.LessonIds!;
        var currentIds = lessons.Select(lesson => lesson.LessonId).ToHashSet();
        if (requestedIds.Count != currentIds.Count || !currentIds.SetEquals(requestedIds))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["lessonIds"] = ["Phải liệt kê đủ và đúng mọi Bài học của khóa, mỗi bài một lần"],
            });
        }

        var byId = lessons.ToDictionary(lesson => lesson.LessonId);
        for (var index = 0; index < requestedIds.Count; index++)
        {
            byId[requestedIds[index]].MoveTo(index + 1);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return [.. lessons.OrderBy(lesson => lesson.LessonOrder).Select(LessonAccess.ToDto)];
    }
}
