using TeduMarket.Application.Identity.Queries.ListUsers;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Tests.Identity;

[Trait("Category", "Unit")]
public sealed class ListUsersQueryValidatorTests
{
    private readonly ListUsersQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    [InlineData(RoleNames.Admin)]
    public void Validate_KnownRoleOrNone_ReturnsValid(string? role)
    {
        // Arrange
        var query = new ListUsersQuery(role);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Teacher", 1, 20, nameof(ListUsersQuery.Role))]
    [InlineData(null, 0, 20, nameof(ListUsersQuery.PageNumber))]
    [InlineData(null, 1, 0, nameof(ListUsersQuery.PageSize))]
    [InlineData(null, 1, 101, nameof(ListUsersQuery.PageSize))]
    public void Validate_InvalidField_ReturnsErrorForThatField(string? role, int pageNumber, int pageSize, string field)
    {
        // Arrange
        var query = new ListUsersQuery(role, pageNumber, pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }
}
