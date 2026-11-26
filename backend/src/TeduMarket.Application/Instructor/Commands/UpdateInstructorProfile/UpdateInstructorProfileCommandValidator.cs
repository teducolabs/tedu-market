using FluentValidation;
using TeduMarket.Domain.Instructor;

namespace TeduMarket.Application.Instructor.Commands.UpdateInstructorProfile;

public sealed class UpdateInstructorProfileCommandValidator : AbstractValidator<UpdateInstructorProfileCommand>
{
    public UpdateInstructorProfileCommandValidator()
    {
        RuleFor(command => command.InstructorProfileId).NotEmpty().WithMessage("Cần chọn Hồ sơ Instructor");
        RuleFor(command => command.DisplayName)
            .Must(displayName => !string.IsNullOrWhiteSpace(displayName)).WithMessage("Không được để trống")
            .Must(displayName => displayName is null || displayName.Trim().Length <= InstructorProfile.DisplayNameMaxLength)
            .WithMessage($"Tối đa {InstructorProfile.DisplayNameMaxLength} ký tự");
        RuleFor(command => command.Biography)
            .Must(biography => biography is null || biography.Trim().Length <= InstructorProfile.BiographyMaxLength)
            .WithMessage($"Tối đa {InstructorProfile.BiographyMaxLength} ký tự");
        RuleFor(command => command.AvatarUrl)
            .Must(avatarUrl => string.IsNullOrWhiteSpace(avatarUrl) || IsHttpUrl(avatarUrl.Trim()))
            .WithMessage("Phải là đường dẫn http hoặc https hợp lệ")
            .Must(avatarUrl => avatarUrl is null || avatarUrl.Trim().Length <= InstructorProfile.AvatarUrlMaxLength)
            .WithMessage($"Tối đa {InstructorProfile.AvatarUrlMaxLength} ký tự");
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
