using Mediator;

namespace TeduMarket.Application.Course.Commands.UpdateCategory;

/// <summary>Admin đổi tên Category (FR-17).</summary>
public sealed record UpdateCategoryCommand(Guid CategoryId, string? Name) : ICommand<CategoryDto>;
