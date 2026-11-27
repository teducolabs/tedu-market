using FluentValidation;

namespace TeduMarket.Application.Course.Commands.SetCourseThumbnail;

public sealed class SetCourseThumbnailCommandValidator : AbstractValidator<SetCourseThumbnailCommand>
{
    public SetCourseThumbnailCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.ObjectKey)
            .Must((command, objectKey) => ThumbnailRules.IsObjectKeyOfCourse(objectKey, command.CourseId))
            .WithMessage("Khóa ảnh không hợp lệ cho Khóa học này");
    }
}
