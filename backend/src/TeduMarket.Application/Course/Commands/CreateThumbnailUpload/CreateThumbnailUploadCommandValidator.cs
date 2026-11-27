using FluentValidation;

namespace TeduMarket.Application.Course.Commands.CreateThumbnailUpload;

public sealed class CreateThumbnailUploadCommandValidator : AbstractValidator<CreateThumbnailUploadCommand>
{
    public CreateThumbnailUploadCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty().WithMessage("Cần chọn Khóa học");
        RuleFor(command => command.ContentType)
            .Must(ThumbnailRules.IsAllowedContentType).WithMessage("Chỉ nhận ảnh JPEG, PNG hoặc WebP");
        RuleFor(command => command.ContentLength)
            .NotNull().WithMessage("Không được để trống")
            .Must(length => length is null or > 0).WithMessage("Dung lượng phải lớn hơn 0")
            .Must(length => length is null or <= ThumbnailRules.MaxContentLength).WithMessage("Dung lượng tối đa 2 MB");
    }
}
