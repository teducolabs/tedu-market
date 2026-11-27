using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.SetCourseThumbnail;

public sealed class SetCourseThumbnailCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    IThumbnailStorage thumbnailStorage,
    TimeProvider timeProvider)
    : ICommandHandler<SetCourseThumbnailCommand, CourseDto>
{
    public async ValueTask<CourseDto> Handle(SetCourseThumbnailCommand command, CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var course = await courseRepository.FindAsync(command.CourseId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Khóa học");
        await CourseAccess.EnsureCanModifyAsync(currentUser, instructorProfileReader, course, cancellationToken);

        if (!await thumbnailStorage.ExistsAsync(command.ObjectKey!, cancellationToken))
        {
            throw new RequestValidationException(
                new Dictionary<string, string[]> { ["objectKey"] = ["Chưa thấy ảnh được tải lên kho lưu trữ"] });
        }

        var previousKey = course.ThumbnailKey;
        course.SetThumbnail(command.ObjectKey, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (previousKey is not null && previousKey != command.ObjectKey)
        {
            await thumbnailStorage.DeleteAsync(previousKey, cancellationToken);
        }

        return CourseAccess.ToDto(course, thumbnailStorage);
    }
}
