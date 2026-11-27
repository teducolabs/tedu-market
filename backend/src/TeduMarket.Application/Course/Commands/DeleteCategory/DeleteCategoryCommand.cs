using Mediator;

namespace TeduMarket.Application.Course.Commands.DeleteCategory;

/// <summary>Admin xóa Category (FR-17).</summary>
public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand;
