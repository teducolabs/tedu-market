using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class AdminSeedTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private const string _adminEmail = "support@tedu.com.vn";

    private readonly string _secretDirectory = Path.Combine(Path.GetTempPath(), $"tedu-seed-{Guid.NewGuid():N}");
    private WebApplicationFactory<Program>? _factory;

    private string SecretFilePath => Path.Combine(_secretDirectory, "secret.json");

    private WebApplicationFactory<Program> Factory => _factory!;

    public async Task InitializeAsync()
    {
        var connectionString = await postgres.CreateDatabaseAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", connectionString);
            builder.UseSetting("AdminSeed:SecretFilePath", SecretFilePath);
        });
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _factory?.Dispose();
        if (Directory.Exists(_secretDirectory))
        {
            Directory.Delete(_secretDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_FirstRun_CreatesAdminWithPasswordFromSecretFileAsync()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Act
        var created = await seeder.SeedAsync(CancellationToken.None);

        // Assert
        Assert.True(created);
        var (email, password) = await ReadSecretFileAsync();
        Assert.Equal(_adminEmail, email);
        var admin = await userManager.FindByEmailAsync(_adminEmail);
        Assert.NotNull(admin);
        Assert.True(await userManager.CheckPasswordAsync(admin, password));
        Assert.True(await userManager.IsInRoleAsync(admin, RoleNames.Admin));
    }

    [Fact]
    public async Task SeedAsync_FirstRun_StoresOnlyPasswordHashAsync()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Act
        await seeder.SeedAsync(CancellationToken.None);

        // Assert
        var (_, password) = await ReadSecretFileAsync();
        var admin = await userManager.FindByEmailAsync(_adminEmail);
        Assert.False(string.IsNullOrEmpty(admin!.PasswordHash));
        Assert.DoesNotContain(password, admin.PasswordHash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SeedAsync_SecondRun_KeepsSingleAdminAndSecretFileAsync()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await seeder.SeedAsync(CancellationToken.None);
        var secretBefore = await File.ReadAllTextAsync(SecretFilePath);

        // Act
        var created = await seeder.SeedAsync(CancellationToken.None);

        // Assert
        Assert.False(created);
        Assert.Single(await userManager.GetUsersInRoleAsync(RoleNames.Admin));
        Assert.Equal(secretBefore, await File.ReadAllTextAsync(SecretFilePath));
    }

    [Fact]
    public async Task StartAsync_Enabled_SeedsAdminAsync()
    {
        // Arrange
        var service = new AdminSeedHostedService(
            Factory.Services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new AdminSeedOptions { Enabled = true }));
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(await userManager.FindByEmailAsync(_adminEmail));
    }

    [Fact]
    public async Task StartAsync_Disabled_DoesNotSeedAdminAsync()
    {
        // Arrange
        var service = new AdminSeedHostedService(
            Factory.Services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new AdminSeedOptions { Enabled = false }));
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        Assert.Null(await userManager.FindByEmailAsync(_adminEmail));
        Assert.False(File.Exists(SecretFilePath));
    }

    private async Task<(string Email, string Password)> ReadSecretFileAsync()
    {
        var json = JsonDocument.Parse(await File.ReadAllTextAsync(SecretFilePath)).RootElement.GetProperty("AdminSeed");
        return (json.GetProperty("Email").GetString()!, json.GetProperty("Password").GetString()!);
    }
}
