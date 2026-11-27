using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Domain.Course;

namespace TeduMarket.Application.Course;

internal static class CategoryNameRules
{
    /// <summary>Tên đã có Category khác dùng thì trả lỗi 400 gắn vào trường <c>name</c> (FR-26).</summary>
    public static async Task EnsureNameAvailableAsync(
        ICategoryRepository categoryRepository, string name, Guid? excludedCategoryId, CancellationToken cancellationToken)
    {
        if (await categoryRepository.ExistsWithNameAsync(name.Trim(), excludedCategoryId, cancellationToken))
        {
            throw new RequestValidationException(new Dictionary<string, string[]> { ["name"] = ["Tên Category đã tồn tại"] });
        }
    }
}
