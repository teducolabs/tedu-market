namespace TeduMarket.Application.Course;

/// <summary>Cổng đọc phía Application; Infrastructure cài đặt bằng Dapper.</summary>
public interface ICategoryReader
{
    /// <summary>Mọi Category, xếp theo tên.</summary>
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken);
}
