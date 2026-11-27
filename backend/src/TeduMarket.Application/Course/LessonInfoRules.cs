using FluentValidation;

namespace TeduMarket.Application.Course;

internal static class LessonInfoRules
{
    public static IRuleBuilderOptions<T, string?> MustBeLessonTitle<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("Không được để trống")
            .Must(title => title is null || title.Trim().Length <= Domain.Course.Lesson.LessonTitleMaxLength)
            .WithMessage($"Tối đa {Domain.Course.Lesson.LessonTitleMaxLength} ký tự");
}
