namespace TeduMarket.Domain.Course;

/// <summary>Bài học: đơn vị nội dung có thứ tự trong một Khóa học; có thể đánh dấu Bài học xem thử (FR-20, FR-45).</summary>
public sealed class Lesson
{
    public const int LessonTitleMaxLength = 200;

    private Lesson()
    {
    }

    public Guid LessonId { get; private set; }

    /// <summary>Id Khóa học chứa bài; không đổi sau khi tạo.</summary>
    public Guid CourseId { get; private set; }

    public string LessonTitle { get; private set; } = string.Empty;

    /// <summary>Thứ tự trong khóa, số nguyên liên tục từ 1.</summary>
    public int LessonOrder { get; private set; }

    /// <summary>Có phải Bài học xem thử (xem không cần mua) hay không; mặc định không.</summary>
    public bool IsPreview { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Lesson Create(Guid courseId, string? lessonTitle, int lessonOrder, DateTimeOffset createdAt)
    {
        if (courseId == Guid.Empty)
        {
            throw new Common.DomainException("INVALID_LESSON", "Bài học phải thuộc một Khóa học");
        }

        if (lessonOrder < 1)
        {
            throw new Common.DomainException("INVALID_LESSON", "Thứ tự bài học bắt đầu từ 1");
        }

        return new Lesson
        {
            LessonId = Guid.CreateVersion7(),
            CourseId = courseId,
            LessonTitle = NormalizeTitle(lessonTitle),
            LessonOrder = lessonOrder,
            CreatedAt = createdAt,
        };
    }

    public void Rename(string? lessonTitle, DateTimeOffset updatedAt)
    {
        LessonTitle = NormalizeTitle(lessonTitle);
        UpdatedAt = updatedAt;
    }

    /// <summary>Đặt lại vị trí; không đổi nội dung nên không cập nhật <see cref="UpdatedAt"/>.</summary>
    public void MoveTo(int lessonOrder)
    {
        if (lessonOrder < 1)
        {
            throw new Common.DomainException("INVALID_LESSON", "Thứ tự bài học bắt đầu từ 1");
        }

        LessonOrder = lessonOrder;
    }

    public void SetPreview(bool isPreview, DateTimeOffset updatedAt)
    {
        IsPreview = isPreview;
        UpdatedAt = updatedAt;
    }

    private static string NormalizeTitle(string? lessonTitle)
    {
        var trimmed = lessonTitle?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > LessonTitleMaxLength)
        {
            throw new Common.DomainException("INVALID_LESSON", $"Tiêu đề bài học bắt buộc, tối đa {LessonTitleMaxLength} ký tự");
        }

        return trimmed;
    }
}
