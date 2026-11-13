using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TeduMarket.Api.Common;

namespace TeduMarket.Api.IntegrationTests.Common;

[Trait("Category", "Unit")]
public sealed class ApiPoliciesTests
{
    [Fact]
    public void GetPartitionKey_AuthenticatedUser_ReturnsUserKey()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "u1")], "test")),
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        // Act
        var key = ApiPolicies.GetPartitionKey(context);

        // Assert
        Assert.Equal("user:u1", key);
    }

    [Fact]
    public void GetPartitionKey_Anonymous_ReturnsIpKey()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        // Act
        var key = ApiPolicies.GetPartitionKey(context);

        // Assert
        Assert.Equal("ip:10.0.0.1", key);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("http://a.example, http://b.example", 2)]
    public void ParseOrigins_CommaSeparatedValue_ReturnsTrimmedOrigins(string? value, int expectedCount)
    {
        // Arrange

        // Act
        var origins = ApiPolicies.ParseOrigins(value);

        // Assert
        Assert.Equal(expectedCount, origins.Length);
    }
}
