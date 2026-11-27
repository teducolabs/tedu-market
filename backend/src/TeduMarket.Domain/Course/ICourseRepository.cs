namespace TeduMarket.Domain.Course;

public interface ICourseRepository
{
    void Add(Course course);

    Task<Course?> FindAsync(Guid courseId, CancellationToken cancellationToken);

    void Remove(Course course);

    /// <summary>Có Khóa học nào thuộc Category này không (chặn xóa Category đang dùng).</summary>
    Task<bool> ExistsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Các slug đã có bắt đầu bằng <paramref name="prefix"/> (gồm chính nó và các biến thể <c>-2</c>, <c>-3</c>...).</summary>
    Task<IReadOnlySet<string>> ListSlugsWithPrefixAsync(string prefix, CancellationToken cancellationToken);
}
