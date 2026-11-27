using Mediator;

namespace TeduMarket.Application.Course.Commands.CreateCategory;

/// <summary>Admin tạo Category (FR-17).</summary>
public sealed record CreateCategoryCommand(string? Name) : ICommand<CategoryDto>;
