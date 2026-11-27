using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.DeleteCourse;

public sealed class DeleteCourseCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourseCommand>
{
    public async ValueTask<Unit> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var course = await courseRepository.FindAsync(command.CourseId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Khóa học");
        await CourseAccess.EnsureCanModifyAsync(currentUser, instructorProfileReader, course, cancellationToken);
        courseRepository.Remove(course);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
