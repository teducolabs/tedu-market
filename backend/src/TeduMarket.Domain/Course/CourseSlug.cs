using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TeduMarket.Domain.Course;

/// <summary>Sinh <see cref="Course.Slug"/> từ tiêu đề: không dấu, chữ thường, nối bằng gạch ngang.</summary>
public static partial class CourseSlug
{
    public const string Fallback = "khoa-hoc";

    /// <summary>Độ dài tối đa của phần sinh từ tiêu đề; phần còn lại của <see cref="Course.SlugMaxLength"/> chừa cho hậu tố số khi trùng.</summary>
    public const int BaseMaxLength = 100;

    public static string From(string title)
    {
        var decomposed = title.Trim().Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        var slug = NonAlphanumeric().Replace(builder.ToString(), "-").Trim('-');
        if (slug.Length > BaseMaxLength)
        {
            slug = slug[..BaseMaxLength].TrimEnd('-');
        }

        return slug.Length == 0 ? Fallback : slug;
    }

    /// <summary>Chọn slug chưa dùng: <paramref name="baseSlug"/>, rồi <c>-2</c>, <c>-3</c>... so với các slug đã có.</summary>
    public static string MakeUnique(string baseSlug, IReadOnlySet<string> takenSlugs)
    {
        if (!takenSlugs.Contains(baseSlug))
        {
            return baseSlug;
        }

        var suffix = 2;
        while (takenSlugs.Contains($"{baseSlug}-{suffix}"))
        {
            suffix++;
        }

        return $"{baseSlug}-{suffix}";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
