using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: Admin đăng nhập bằng email + mật khẩu (T15, FR-04).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class AdminLoginTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private const string _email = "admin@example.test";
    private const string _password = "Mat-khau-1!";

    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync()
    {
        _host = new IdentityTestHost(await postgres.CreateDatabaseAsync());
        await _host.RegisterSampleClientAsync();
        await Host.CreateUserWithPasswordAsync(_email, _password, RoleNames.Admin);
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
    public async Task LoginPage_Get_ReturnsFormWithOwnContentSecurityPolicyAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.GetAsync("/account/login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"email\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"password\"", html, StringComparison.Ordinal);
        Assert.Contains("style-src 'sha256-", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToReturnUrlAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await IdentityTestHost.SubmitLoginAsync(client, _email, _password, "/connect/authorize?x=1");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/connect/authorize?x=1", response.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("https://evil.example/steal")]
    [InlineData("//evil.example/steal")]
    public async Task Login_ExternalReturnUrl_RedirectsToRootAsync(string returnUrl)
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await IdentityTestHost.SubmitLoginAsync(client, _email, _password, returnUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData(_email, "sai-mat-khau")]
    [InlineData("khong-co@example.test", _password)]
    public async Task Login_InvalidCredentials_ShowsGenericErrorWithoutSigningInAsync(string email, string password)
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await IdentityTestHost.SubmitLoginAsync(client, email, password, "/");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains("Email hoặc mật khẩu chưa đúng", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        var tokenResponse = await client.GetAsync(IdentityTestHost.BuildAuthorizeUrl("openid", IdentityTestHost.CreatePkcePair().Challenge));
        Assert.Equal("/account/login", tokenResponse.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task Login_RepeatedWrongPassword_LocksAccountAsync()
    {
        // Arrange
        using var client = Host.CreateClient();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await IdentityTestHost.SubmitLoginAsync(client, _email, "sai-mat-khau", "/");
        }

        // Act
        var response = await IdentityTestHost.SubmitLoginAsync(client, _email, _password, "/");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("tạm khóa", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_PostWithoutAntiforgeryToken_ReturnsBadRequestAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.PostAsync("/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["email"] = _email,
            ["password"] = _password,
        }));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuthorizationCodeFlow_NotSignedIn_LoginThenTokenAsync()
    {
        // Arrange
        using var client = Host.CreateClient();
        var (verifier, challenge) = IdentityTestHost.CreatePkcePair();
        var authorizeUrl = IdentityTestHost.BuildAuthorizeUrl("openid roles", challenge);
        var toLogin = await client.GetAsync(authorizeUrl);
        var returnUrl = System.Web.HttpUtility.ParseQueryString(toLogin.Headers.Location!.Query)["ReturnUrl"]!;
        var login = await IdentityTestHost.SubmitLoginAsync(client, _email, _password, returnUrl);
        var code = await IdentityTestHost.AuthorizeAsync(client, "openid roles", challenge);

        // Act
        var response = await IdentityTestHost.ExchangeCodeAsync(client, code, verifier);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<JsonElement>();
        using var api = Host.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/v1/samples/admin-only")).StatusCode);
    }
}
