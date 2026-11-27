using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.UpdateCourse;

public sealed class UpdateCourseCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICategoryRepository categoryRepository,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    IThumbnailStorage thumbnailStorage,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateCourseCommand, CourseDto>
{
    public async ValueTask<CourseDto> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var course = await courseRepository.FindAsync(command.CourseId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Khóa học");

        await CourseAccess.EnsureCanModifyAsync(currentUser, instructorProfileReader, course, cancellationToken);
        await CourseAccess.EnsureCategoryExistsAsync(categoryRepository, command.CategoryId, cancellationToken);
        course.Update(command.CategoryId, command.Title, command.Description, command.Price!.Value, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CourseAccess.ToDto(course, thumbnailStorage);
    }
}
