namespace TeduMarket.Application.Course;

/// <param name="CategoryId">Id Category.</param>
/// <param name="Name">Tên Category.</param>
public sealed record CategoryDto(Guid CategoryId, string Name);
