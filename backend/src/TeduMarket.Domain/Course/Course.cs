namespace TeduMarket.Domain.Course;

/// <summary>Khóa học: sản phẩm bán, thuộc 1 Instructor (qua Hồ sơ Instructor) và 1 Category (FR-19).</summary>
public sealed class Course
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 5000;
    public const int SlugMaxLength = 120;

    /// <summary>Giá lớn nhất lưu được trong cột <c>numeric(18,0)</c>.</summary>
    public const decimal PriceMax = 999_999_999_999_999_999m;

    private Course()
    {
    }

    public Guid CourseId { get; private set; }

    /// <summary>Id Hồ sơ Instructor sở hữu khóa; không đổi sau khi tạo.</summary>
    public Guid InstructorProfileId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    /// <summary>Đường dẫn công khai, sinh lúc tạo từ tiêu đề và không đổi khi sửa tiêu đề.</summary>
    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public CourseStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>Tạo khóa mới ở trạng thái <see cref="CourseStatus.Draft"/>.</summary>
    public static Course Create(
        Guid instructorProfileId, Guid categoryId, string? title, string slug, string? description, decimal price, DateTimeOffset createdAt)
    {
        if (instructorProfileId == Guid.Empty)
        {
            throw new Common.DomainException("INVALID_COURSE", "Khóa học phải thuộc một Instructor");
        }

        if (string.IsNullOrWhiteSpace(slug) || slug.Length > SlugMaxLength)
        {
            throw new Common.DomainException("INVALID_COURSE", $"Slug bắt buộc, tối đa {SlugMaxLength} ký tự");
        }

        var course = new Course
        {
            Slug = slug,
            CourseId = Guid.CreateVersion7(),
            InstructorProfileId = instructorProfileId,
            Status = CourseStatus.Draft,
            CreatedAt = createdAt,
        };
        course.Apply(categoryId, title, description, price);
        return course;
    }

    /// <summary>Thay toàn bộ thông tin, giá và Category; không đổi chủ khóa và trạng thái.</summary>
    public void Update(Guid categoryId, string? title, string? description, decimal price, DateTimeOffset updatedAt)
    {
        Apply(categoryId, title, description, price);
        UpdatedAt = updatedAt;
    }

    private void Apply(Guid categoryId, string? title, string? description, decimal price)
    {
        var trimmedTitle = title?.Trim();
        if (string.IsNullOrEmpty(trimmedTitle) || trimmedTitle.Length > TitleMaxLength)
        {
            throw new Common.DomainException("INVALID_COURSE", $"Tiêu đề khóa học bắt buộc, tối đa {TitleMaxLength} ký tự");
        }

        var trimmedDescription = description?.Trim();
        if (trimmedDescription is { Length: > DescriptionMaxLength })
        {
            throw new Common.DomainException("INVALID_COURSE", $"Mô tả khóa học tối đa {DescriptionMaxLength} ký tự");
        }

        if (categoryId == Guid.Empty)
        {
            throw new Common.DomainException("INVALID_COURSE", "Khóa học phải thuộc một Category");
        }

        if (price < 0 || price > PriceMax || price != decimal.Truncate(price))
        {
            throw new Common.DomainException("INVALID_COURSE", "Giá phải là số nguyên không âm");
        }

        CategoryId = categoryId;
        Title = trimmedTitle;
        Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;
        Price = price;
    }
}
