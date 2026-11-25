using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class IdentityFlowTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync()
    {
        _host = new IdentityTestHost(await postgres.CreateDatabaseAsync());
        await _host.RegisterSampleClientAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    public void Dispose() => _host?.Dispose();

    [Fact]
    public async Task Migration_Applied_SeedsThreeRolesAsync()
    {
        // Arrange
        await using var scope = Host.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        // Act
        var roleNames = roleManager.Roles.Select(role => role.Name).OrderBy(name => name).ToList();

        // Assert
        Assert.Equal([RoleNames.Admin, RoleNames.Instructor, RoleNames.Student], roleNames);
    }

    [Fact]
    public async Task AuthorizationCodeFlow_ValidPkce_ReturnsAccessTokenWithRolesAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        using var client = Host.CreateClient();
        await IdentityTestHost.SignInAsync(client, userId);
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();
        var code = await IdentityTestHost.AuthorizeAsync(client, "openid roles", challenge);

        // Act
        var response = await IdentityTestHost.ExchangeCodeAsync(client, code, verifier);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(tokens.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrEmpty(tokens.GetProperty("id_token").GetString()));
    }

    [Fact]
    public async Task TokenEndpoint_WrongCodeVerifier_ReturnsInvalidGrantAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        using var client = Host.CreateClient();
        await IdentityTestHost.SignInAsync(client, userId);
        var (_, challenge) = IdentityTestHost.CreatePkcePair();
        var code = await IdentityTestHost.AuthorizeAsync(client, "openid", challenge);
        var (otherVerifier, _) = IdentityTestHost.CreatePkcePair();

        // Act
        var response = await IdentityTestHost.ExchangeCodeAsync(client, code, otherVerifier);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("invalid_grant", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task AuthorizeEndpoint_MissingCodeChallenge_ReturnsInvalidRequestAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        using var client = Host.CreateClient();
        await IdentityTestHost.SignInAsync(client, userId);

        // Act
        var response = await client.GetAsync(IdentityTestHost.BuildAuthorizeUrl("openid", codeChallenge: null));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains("invalid_request", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthorizeEndpoint_NotSignedIn_RedirectsToLoginPageAsync()
    {
        // Arrange
        using var client = Host.CreateClient();
        var (_, challenge) = IdentityTestHost.CreatePkcePair();

        // Act
        var response = await client.GetAsync(IdentityTestHost.BuildAuthorizeUrl("openid", challenge));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/account/login", response.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task TokenEndpoint_RefreshToken_ReturnsNewAccessTokenAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        var tokens = await Host.LoginAsync(userId);
        using var client = Host.CreateClient();

        // Act
        var response = await IdentityTestHost.RefreshAsync(client, tokens.GetProperty("refresh_token").GetString()!);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(refreshed.GetProperty("access_token").GetString()));
    }

    [Fact]
    public async Task TokenEndpoint_RefreshTokenOfDeletedUser_ReturnsInvalidGrantAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        var tokens = await Host.LoginAsync(userId);
        await Host.DeleteUserAsync(userId);
        using var client = Host.CreateClient();

        // Act
        var response = await IdentityTestHost.RefreshAsync(client, tokens.GetProperty("refresh_token").GetString()!);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("invalid_grant", body.GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("/api/v1/samples/admin-only", HttpStatusCode.Unauthorized, null)]
    [InlineData("/api/v1/samples/admin-only", HttpStatusCode.Forbidden, RoleNames.Student)]
    [InlineData("/api/v1/samples/admin-only", HttpStatusCode.Forbidden, RoleNames.Instructor)]
    [InlineData("/api/v1/samples/admin-only", HttpStatusCode.OK, RoleNames.Admin)]
    [InlineData("/api/v1/samples/instructor-only", HttpStatusCode.Forbidden, RoleNames.Student)]
    [InlineData("/api/v1/samples/instructor-only", HttpStatusCode.OK, RoleNames.Instructor)]
    [InlineData("/api/v1/samples/student-only", HttpStatusCode.Forbidden, RoleNames.Admin)]
    [InlineData("/api/v1/samples/student-only", HttpStatusCode.OK, RoleNames.Student)]
    public async Task Endpoint_WithRolePolicy_ReturnsExpectedStatusAsync(string path, HttpStatusCode expected, string? role)
    {
        // Arrange
        using var client = Host.CreateClient();
        if (role is not null)
        {
            var token = await Host.GetAccessTokenAsync(role);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Act
        var response = await client.GetAsync(path);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_InstructorWithStudentRole_PassesStudentPolicyAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student, RoleNames.Instructor);
        var tokens = await Host.LoginAsync(userId);
        using var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());

        // Act
        var response = await client.GetAsync("/api/v1/samples/student-only");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Handler_OwnedResourceOfAnotherUser_ReturnsForbiddenAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        var tokens = await Host.LoginAsync(userId);
        using var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());

        // Act
        var response = await client.GetAsync($"/api/v1/samples/owned/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FORBIDDEN", body.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Handler_OwnedResourceOfSameUser_ReturnsOkAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        var tokens = await Host.LoginAsync(userId);
        using var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());

        // Act
        var response = await client.GetAsync($"/api/v1/samples/owned/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Handler_OwnedResourceOfAnotherUserAsAdmin_ReturnsOkAsync()
    {
        // Arrange
        var token = await Host.GetAccessTokenAsync(RoleNames.Admin);
        using var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/v1/samples/owned/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
