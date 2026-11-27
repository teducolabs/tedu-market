namespace TeduMarket.Domain.Course;

/// <summary>Nhóm phân loại Khóa học do Admin quản lý, dùng để lọc danh sách (FR-17).</summary>
public sealed class Category
{
    public const int NameMaxLength = 100;

    private Category()
    {
    }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Tên viết hoa theo ngôn ngữ bất biến, dùng cho unique index: hai Category không trùng tên kể cả khác hoa thường.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Category Create(string? name, DateTimeOffset createdAt) =>
        new Category { CategoryId = Guid.CreateVersion7(), CreatedAt = createdAt }.WithName(name);

    public void Rename(string? name, DateTimeOffset updatedAt)
    {
        WithName(name);
        UpdatedAt = updatedAt;
    }

    /// <summary>Khóa so sánh tên Category không phân biệt hoa thường.</summary>
    public static string ToNormalizedName(string name) => name.Trim().ToUpperInvariant();

    private Category WithName(string? name)
    {
        Name = NormalizeName(name);
        NormalizedName = ToNormalizedName(Name);
        return this;
    }

    private static string NormalizeName(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > NameMaxLength)
        {
            throw new Common.DomainException("INVALID_CATEGORY", $"Tên Category bắt buộc, tối đa {NameMaxLength} ký tự");
        }

        return trimmed;
    }
}
