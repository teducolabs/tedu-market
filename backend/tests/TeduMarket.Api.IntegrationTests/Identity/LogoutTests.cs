using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: đăng xuất thu hồi token (T15, FR-04).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class LogoutTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Logout_WithIdTokenHint_RevokesAccessTokenAndRedirectsAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Admin);
        var tokens = await Host.LoginAsync(userId);
        using var api = BearerClient(tokens);
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/v1/samples/admin-only")).StatusCode);
        using var browser = Host.CreateClient();

        // Act
        var response = await browser.GetAsync(LogoutUrl(tokens.GetProperty("id_token").GetString()!));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(IdentityTestHost.PostLogoutRedirectUri, response.Headers.Location!.OriginalString, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/v1/samples/admin-only")).StatusCode);
    }

    [Fact]
    public async Task Logout_AfterwardsRefreshTokenIsRejectedAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Admin);
        var tokens = await Host.LoginAsync(userId);
        using var browser = Host.CreateClient();
        await browser.GetAsync(LogoutUrl(tokens.GetProperty("id_token").GetString()!));

        // Act
        var response = await IdentityTestHost.RefreshAsync(browser, tokens.GetProperty("refresh_token").GetString()!);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithSessionCookieOnly_RevokesTokensAndEndsSessionAsync()
    {
        // Arrange
        var userId = await Host.CreateUserAsync(RoleNames.Admin);
        using var browser = Host.CreateClient();
        await IdentityTestHost.SignInAsync(browser, userId);
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();
        var code = await IdentityTestHost.AuthorizeAsync(browser, "openid roles", challenge);
        var tokens = await (await IdentityTestHost.ExchangeCodeAsync(browser, code, verifier)).Content.ReadFromJsonAsync<JsonElement>();
        using var api = BearerClient(tokens);

        // Act
        await browser.GetAsync("/connect/logout");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/v1/samples/admin-only")).StatusCode);
        var again = await browser.GetAsync(IdentityTestHost.BuildAuthorizeUrl("openid", IdentityTestHost.CreatePkcePair().Challenge));
        Assert.Equal("/account/login", again.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task Logout_DoesNotRevokeOtherUsersTokensAsync()
    {
        // Arrange
        var loggingOut = await Host.LoginAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var other = await Host.LoginAsync(await Host.CreateUserAsync(RoleNames.Admin));
        using var otherApi = BearerClient(other);
        using var browser = Host.CreateClient();

        // Act
        await browser.GetAsync(LogoutUrl(loggingOut.GetProperty("id_token").GetString()!));

        // Assert
        Assert.Equal(HttpStatusCode.OK, (await otherApi.GetAsync("/api/v1/samples/admin-only")).StatusCode);
    }

    private static string LogoutUrl(string idToken) =>
        $"/connect/logout?id_token_hint={Uri.EscapeDataString(idToken)}&post_logout_redirect_uri={Uri.EscapeDataString(IdentityTestHost.PostLogoutRedirectUri)}";

    private HttpClient BearerClient(JsonElement tokens)
    {
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
