using Mediator;
using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Instructor;
using TeduMarket.Domain.Common;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Course.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler(
    ICurrentUser currentUser,
    IInstructorProfileReader instructorProfileReader,
    ICategoryRepository categoryRepository,
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateCourseCommand, CourseDto>
{
    public async ValueTask<CourseDto> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        CourseAccess.EnsureCanWrite(currentUser);
        var instructorProfileId = await ResolveInstructorProfileIdAsync(command, cancellationToken);
        await CourseAccess.EnsureCategoryExistsAsync(categoryRepository, command.CategoryId, cancellationToken);

        var baseSlug = CourseSlug.From(command.Title!);
        var slug = CourseSlug.MakeUnique(baseSlug, await courseRepository.ListSlugsWithPrefixAsync(baseSlug, cancellationToken));
        var course = Domain.Course.Course.Create(
            instructorProfileId, command.CategoryId, command.Title, slug, command.Description, command.Price!.Value, timeProvider.GetUtcNow());
        courseRepository.Add(course);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CourseAccess.ToDto(course);
    }

    private async Task<Guid> ResolveInstructorProfileIdAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole(RoleNames.Admin))
        {
            if (command.InstructorProfileId is not { } requestedId
                || await instructorProfileReader.FindAsync(requestedId, cancellationToken) is null)
            {
                throw new RequestValidationException(
                    new Dictionary<string, string[]> { ["instructorProfileId"] = ["Cần chọn Hồ sơ Instructor hợp lệ"] });
            }

            return requestedId;
        }

        var ownProfileId = await CourseAccess.FindOwnProfileIdAsync(currentUser, instructorProfileReader, cancellationToken)
            ?? throw new ForbiddenException("Tài khoản chưa có Hồ sơ Instructor");
        if (command.InstructorProfileId is { } otherId && otherId != ownProfileId)
        {
            throw new ForbiddenException("Instructor chỉ được tạo khóa học của chính mình");
        }

        return ownProfileId;
    }
}
