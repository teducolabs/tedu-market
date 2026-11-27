using Mediator;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Queries.ListLessons;

public sealed class ListLessonsQueryHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    ILessonReader lessonReader)
    : IQueryHandler<ListLessonsQuery, IReadOnlyList<LessonDto>>
{
    public async ValueTask<IReadOnlyList<LessonDto>> Handle(ListLessonsQuery query, CancellationToken cancellationToken)
    {
        await LessonAccess.FindCourseForModifyAsync(currentUser, instructorProfileReader, courseRepository, query.CourseId, cancellationToken);
        return await lessonReader.ListByCourseAsync(query.CourseId, cancellationToken);
    }
}
