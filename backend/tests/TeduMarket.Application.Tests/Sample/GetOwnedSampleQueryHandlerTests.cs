using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Sample.Queries.GetOwnedSample;

namespace TeduMarket.Application.Tests.Sample;

[Trait("Category", "Unit")]
public sealed class GetOwnedSampleQueryHandlerTests
{
    [Fact]
    public async Task Handle_CurrentUserIsOwner_ReturnsOwnedSampleAsync()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var handler = new GetOwnedSampleQueryHandler(new FakeCurrentUser(ownerId));

        // Act
        var result = await handler.Handle(new GetOwnedSampleQuery(ownerId), CancellationToken.None);

        // Assert
        Assert.Equal(ownerId, result.OwnerId);
    }

    [Fact]
    public async Task Handle_CurrentUserIsNotOwner_ThrowsForbiddenAsync()
    {
        // Arrange
        var handler = new GetOwnedSampleQueryHandler(new FakeCurrentUser(Guid.NewGuid()));

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await handler.Handle(new GetOwnedSampleQuery(Guid.NewGuid()), CancellationToken.None));

        // Assert
        Assert.IsType<ForbiddenException>(exception);
    }

    private sealed class FakeCurrentUser(Guid? userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public bool IsInRole(string roleName) => false;
    }
}
