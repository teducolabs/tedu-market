using TeduMarket.Application.Sample.Commands.CreateSample;

namespace TeduMarket.Application.Tests.Sample;

[Trait("Category", "Unit")]
public sealed class CreateSampleCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsDtoWithTitleAsync()
    {
        // Arrange
        var handler = new CreateSampleCommandHandler();

        // Act
        var result = await handler.Handle(new CreateSampleCommand("Tiêu đề"), CancellationToken.None);

        // Assert
        Assert.Equal("Tiêu đề", result.Title);
    }
}
