namespace TeduMarket.Domain.Course;

public interface ICategoryRepository
{
    void Add(Category category);

    Task<Category?> FindAsync(Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Có Category nào khác <paramref name="excludedCategoryId"/> trùng tên (không phân biệt hoa thường) không.</summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? excludedCategoryId, CancellationToken cancellationToken);

    void Remove(Category category);
}
