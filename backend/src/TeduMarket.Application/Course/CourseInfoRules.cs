using FluentValidation;

namespace TeduMarket.Application.Course;

internal static class CourseInfoRules
{
    public static IRuleBuilderOptions<T, Guid> MustBeCategory<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEmpty().WithMessage("Cần chọn Category");

    public static IRuleBuilderOptions<T, string?> MustBeTitle<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("Không được để trống")
            .Must(title => title is null || title.Trim().Length <= Domain.Course.Course.TitleMaxLength)
            .WithMessage($"Tối đa {Domain.Course.Course.TitleMaxLength} ký tự");

    public static IRuleBuilderOptions<T, string?> MustBeDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(description => description is null || description.Trim().Length <= Domain.Course.Course.DescriptionMaxLength)
            .WithMessage($"Tối đa {Domain.Course.Course.DescriptionMaxLength} ký tự");

    public static IRuleBuilderOptions<T, decimal?> MustBePrice<T>(this IRuleBuilder<T, decimal?> rule) =>
        rule.NotNull().WithMessage("Không được để trống")
            .Must(price => price is null || (price >= 0 && price <= Domain.Course.Course.PriceMax && price == decimal.Truncate(price.Value)))
            .WithMessage("Phải là số nguyên không âm");
}
