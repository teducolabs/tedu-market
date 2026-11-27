using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course.Commands.CreateThumbnailUpload;

public sealed class CreateThumbnailUploadCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICourseRepository courseRepository,
    IThumbnailStorage thumbnailStorage)
    : ICommandHandler<CreateThumbnailUploadCommand, ThumbnailUploadDto>
{
    public async ValueTask<ThumbnailUploadDto> Handle(CreateThumbnailUploadCommand command, CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var course = await courseRepository.FindAsync(command.CourseId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy Khóa học");
        await CourseAccess.EnsureCanModifyAsync(currentUser, instructorProfileReader, course, cancellationToken);

        var contentType = command.ContentType!.ToLowerInvariant();
        var contentLength = command.ContentLength!.Value;
        var objectKey = ThumbnailRules.NewObjectKey(course.CourseId, contentType);
        var ticket = thumbnailStorage.CreateUpload(objectKey, contentType, contentLength);
        return new ThumbnailUploadDto(ticket.UploadUrl, objectKey, contentType, contentLength, ticket.ExpiresAt);
    }
}
