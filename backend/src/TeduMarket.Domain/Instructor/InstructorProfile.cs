namespace TeduMarket.Domain.Instructor;

/// <summary>Hồ sơ Instructor, tạo trống khi Admin duyệt Instructor (FR-40); Instructor hoặc Admin điền và sửa nội dung hiển thị (FR-18, FR-43).</summary>
public sealed class InstructorProfile
{
    public const int DisplayNameMaxLength = 100;
    public const int BiographyMaxLength = 2000;
    public const int AvatarUrlMaxLength = 2048;

    private InstructorProfile()
    {
    }

    public Guid InstructorProfileId { get; private set; }

    /// <summary>Id tài khoản <c>User</c> của Instructor; mỗi tài khoản có tối đa 1 hồ sơ.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Tên hiển thị trên khóa học; <c>null</c> cho tới lần cập nhật đầu tiên.</summary>
    public string? DisplayName { get; private set; }

    public string? Biography { get; private set; }

    public string? AvatarUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static InstructorProfile Create(Guid userId, DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new Common.DomainException("INVALID_INSTRUCTOR_USER", "Cần chọn người dùng để duyệt Instructor");
        }

        return new InstructorProfile { InstructorProfileId = Guid.CreateVersion7(), UserId = userId, CreatedAt = createdAt };
    }

    /// <summary>Thay toàn bộ nội dung hiển thị; chuỗi rỗng của giới thiệu và ảnh đại diện được lưu là <c>null</c>.</summary>
    public void UpdateContent(string? displayName, string? biography, string? avatarUrl, DateTimeOffset updatedAt)
    {
        var trimmedDisplayName = displayName?.Trim();
        if (string.IsNullOrEmpty(trimmedDisplayName) || trimmedDisplayName.Length > DisplayNameMaxLength)
        {
            throw new Common.DomainException("INVALID_INSTRUCTOR_PROFILE", $"Tên hiển thị bắt buộc, tối đa {DisplayNameMaxLength} ký tự");
        }

        var trimmedBiography = NullIfBlank(biography);
        if (trimmedBiography is { Length: > BiographyMaxLength })
        {
            throw new Common.DomainException("INVALID_INSTRUCTOR_PROFILE", $"Giới thiệu tối đa {BiographyMaxLength} ký tự");
        }

        var trimmedAvatarUrl = NullIfBlank(avatarUrl);
        if (trimmedAvatarUrl is not null && !IsHttpUrl(trimmedAvatarUrl))
        {
            throw new Common.DomainException("INVALID_INSTRUCTOR_PROFILE", "Ảnh đại diện phải là đường dẫn http hoặc https hợp lệ");
        }

        DisplayName = trimmedDisplayName;
        Biography = trimmedBiography;
        AvatarUrl = trimmedAvatarUrl;
        UpdatedAt = updatedAt;
    }

    private static string? NullIfBlank(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool IsHttpUrl(string value) =>
        value.Length <= AvatarUrlMaxLength
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
