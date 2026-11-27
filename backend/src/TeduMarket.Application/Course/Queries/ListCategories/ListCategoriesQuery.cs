using Mediator;

namespace TeduMarket.Application.Course.Queries.ListCategories;

/// <summary>Danh sách Category công khai (Khách cũng gọi được) để lọc Khóa học và để Admin quản lý.</summary>
public sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;
