using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Infrastructure.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeduMarket.Api.IntegrationTests.Identity;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class OidcClientSeederTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync() => _host = new IdentityTestHost(await postgres.CreateDatabaseAsync());

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    public void Dispose() => _host?.Dispose();

    [Fact]
    public async Task StartAsync_ConfiguredClients_RegistersPublicAdminAppAndConfidentialStudentWebAsync()
    {
        // Arrange
        var seeder = CreateSeeder(
            new OidcClientOptions { RedirectUris = ["http://localhost:5173/auth/callback"], PostLogoutRedirectUris = ["http://localhost:5173/"] },
            new OidcClientOptions { RedirectUris = ["http://localhost:3000/auth/callback"], OidcClientSecret = "secret-cua-student-web" });

        // Act
        await seeder.StartAsync(CancellationToken.None);

        // Assert
        await using var scope = Host.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var admin = await manager.FindByClientIdAsync(OidcClientSeeder.AdminAppClientId);
        var student = await manager.FindByClientIdAsync(OidcClientSeeder.StudentWebClientId);
        Assert.Equal(ClientTypes.Public, await manager.GetClientTypeAsync(admin!));
        Assert.Equal(ClientTypes.Confidential, await manager.GetClientTypeAsync(student!));
        Assert.Contains("http://localhost:5173/auth/callback", await manager.GetRedirectUrisAsync(admin!));
        Assert.Contains("http://localhost:5173/", await manager.GetPostLogoutRedirectUrisAsync(admin!));
        Assert.True(await manager.HasPermissionAsync(admin!, Permissions.Endpoints.EndSession));
        Assert.True(await manager.HasRequirementAsync(admin!, Requirements.Features.ProofKeyForCodeExchange));
        Assert.True(await manager.ValidateClientSecretAsync(student!, "secret-cua-student-web"));
    }

    [Fact]
    public async Task StartAsync_RunTwiceWithChangedRedirectUri_UpdatesClientWithoutDuplicateAsync()
    {
        // Arrange
        await CreateSeeder(new OidcClientOptions { RedirectUris = ["http://localhost:5173/old"] }, new OidcClientOptions()).StartAsync(CancellationToken.None);
        var second = CreateSeeder(new OidcClientOptions { RedirectUris = ["http://localhost:5173/new"] }, new OidcClientOptions());

        // Act
        await second.StartAsync(CancellationToken.None);

        // Assert
        await using var scope = Host.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        Assert.Equal(1, await manager.CountAsync());
        var admin = await manager.FindByClientIdAsync(OidcClientSeeder.AdminAppClientId);
        Assert.Equal("http://localhost:5173/new", Assert.Single(await manager.GetRedirectUrisAsync(admin!)));
    }

    [Fact]
    public async Task StartAsync_StudentWebWithoutSecret_SkipsStudentWebAsync()
    {
        // Arrange
        var seeder = CreateSeeder(new OidcClientOptions(), new OidcClientOptions { RedirectUris = ["http://localhost:3000/auth/callback"] });

        // Act
        await seeder.StartAsync(CancellationToken.None);

        // Assert
        await using var scope = Host.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        Assert.Null(await manager.FindByClientIdAsync(OidcClientSeeder.StudentWebClientId));
    }

    private OidcClientSeeder CreateSeeder(OidcClientOptions adminApp, OidcClientOptions studentWeb) =>
        new(Host.Services.GetRequiredService<IServiceScopeFactory>(), new FakeOptionsMonitor(adminApp, studentWeb), NullLogger<OidcClientSeeder>.Instance);

    private sealed class FakeOptionsMonitor(OidcClientOptions adminApp, OidcClientOptions studentWeb) : IOptionsMonitor<OidcClientOptions>
    {
        public OidcClientOptions CurrentValue => adminApp;

        public OidcClientOptions Get(string? name) => name == OidcClientOptions.StudentWebSectionName ? studentWeb : adminApp;

        public IDisposable? OnChange(Action<OidcClientOptions, string?> listener) => null;
    }
}
