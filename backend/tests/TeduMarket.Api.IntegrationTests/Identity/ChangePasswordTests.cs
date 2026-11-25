using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: Admin đổi mật khẩu (T15, FR-42).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class ChangePasswordTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private const string _email = "admin@example.test";
    private const string _oldPassword = "Mat-khau-1!";
    private const string _newPassword = "Mat-khau-moi-2@";

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
    public async Task ChangePassword_ValidRequest_OnlyNewPasswordWorksAfterwardsAsync()
    {
        // Arrange
        var userId = await Host.CreateUserWithPasswordAsync(_email, _oldPassword, RoleNames.Admin);
        using var api = await CreateAuthorizedClientAsync(userId);

        // Act
        var response = await api.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = _oldPassword, newPassword = _newPassword });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var oldLogin = Host.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await IdentityTestHost.SubmitLoginAsync(oldLogin, _email, _oldPassword, "/")).StatusCode);
        using var newLogin = Host.CreateClient();
        Assert.Equal(HttpStatusCode.Redirect, (await IdentityTestHost.SubmitLoginAsync(newLogin, _email, _newPassword, "/")).StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ValidRequest_RevokesExistingTokensAsync()
    {
        // Arrange
        var userId = await Host.CreateUserWithPasswordAsync(_email, _oldPassword, RoleNames.Admin);
        var tokens = await Host.LoginAsync(userId);
        using var api = Host.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());

        // Act
        await api.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = _oldPassword, newPassword = _newPassword });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/v1/samples/admin-only")).StatusCode);
        var refresh = await IdentityTestHost.RefreshAsync(Host.CreateClient(), tokens.GetProperty("refresh_token").GetString()!);
        Assert.Equal(HttpStatusCode.BadRequest, refresh.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsFieldErrorAndKeepsOldPasswordAsync()
    {
        // Arrange
        var userId = await Host.CreateUserWithPasswordAsync(_email, _oldPassword, RoleNames.Admin);
        using var api = await CreateAuthorizedClientAsync(userId);

        // Act
        var response = await api.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = "sai-mat-khau", newPassword = _newPassword });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INVALID_REQUEST", body.GetProperty("errorCode").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("currentPassword", out _));
        using var login = Host.CreateClient();
        Assert.Equal(HttpStatusCode.Redirect, (await IdentityTestHost.SubmitLoginAsync(login, _email, _oldPassword, "/")).StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WeakNewPassword_ReturnsFieldErrorAsync()
    {
        // Arrange
        var userId = await Host.CreateUserWithPasswordAsync(_email, _oldPassword, RoleNames.Admin);
        using var api = await CreateAuthorizedClientAsync(userId);

        // Act
        var response = await api.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = _oldPassword, newPassword = "abc" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("newPassword", out var errors));
        Assert.NotEqual(0, errors.GetArrayLength());
    }

    [Fact]
    public async Task ChangePassword_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = _oldPassword, newPassword = _newPassword });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    public async Task ChangePassword_NotAdmin_ReturnsForbiddenAsync(string role)
    {
        // Arrange
        var userId = await Host.CreateUserAsync(role);
        using var api = await CreateAuthorizedClientAsync(userId);

        // Act
        var response = await api.PostAsJsonAsync("/api/v1/account/change-password", new { currentPassword = _oldPassword, newPassword = _newPassword });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
