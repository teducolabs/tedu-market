using TeduMarket.Application.Common.Exceptions;
using TeduMarket.Application.Common.Paging;
using TeduMarket.Application.Identity;
using TeduMarket.Application.Identity.Queries.ListUsers;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Identity;

[Trait("Category", "Unit")]
public sealed class ListUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_Admin_ReturnsReaderResultForFilterAndPageAsync()
    {
        // Arrange
        var expected = new PagedResult<UserDto>([new UserDto(Guid.NewGuid(), "a@example.test", [RoleNames.Student], null)], 2, 10, 11);
        var reader = new FakeUserReader(expected);
        var handler = new ListUsersQueryHandler(new FakeCurrentUser(RoleNames.Admin), reader);

        // Act
        var result = await handler.Handle(new ListUsersQuery(RoleNames.Student, 2, 10), CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        Assert.Equal((RoleNames.Student, 2, 10), reader.Call);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    public async Task Handle_NotAdmin_ThrowsForbiddenWithoutReadingAsync(string role)
    {
        // Arrange
        var reader = new FakeUserReader(new PagedResult<UserDto>([], 1, 20, 0));
        var handler = new ListUsersQueryHandler(new FakeCurrentUser(role), reader);

        // Act
        var act = async () => await handler.Handle(new ListUsersQuery(null), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Null(reader.Call);
    }

    private sealed class FakeCurrentUser(string role) : ICurrentUser
    {
        public Guid? UserId { get; } = Guid.NewGuid();

        public bool IsInRole(string roleName) => roleName == role;
    }

    private sealed class FakeUserReader(PagedResult<UserDto> result) : IUserReader
    {
        public (string? Role, int PageNumber, int PageSize)? Call { get; private set; }

        public Task<PagedResult<UserDto>> ListAsync(string? roleName, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            Call = (roleName, pageNumber, pageSize);
            return Task.FromResult(result);
        }
    }
}
