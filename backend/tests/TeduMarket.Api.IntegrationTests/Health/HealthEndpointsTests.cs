using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Infrastructure.Health;
using Testcontainers.PostgreSql;

namespace TeduMarket.Api.IntegrationTests.Health;

[Trait("Category", "Integration")]
public sealed class HealthEndpointsTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();
    private readonly StubMinioHandler _minio = new();
    private WebApplicationFactory<Program>? _factory;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", connectionString);
            builder.ConfigureServices(services =>
                services.AddHttpClient(MinioHealthCheck.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => _minio));
        });
    }

    public void Dispose() => _minio.Dispose();

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task GetHealthLive_DependenciesDown_ReturnsOkAsync()
    {
        // Arrange
        _minio.StatusCode = HttpStatusCode.ServiceUnavailable;
        await _postgres.StopAsync();
        using var client = _factory!.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_PostgresAndMinioUp_ReturnsOkAsync()
    {
        // Arrange
        using var client = _factory!.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_MinioDown_ReturnsServiceUnavailableAsync()
    {
        // Arrange
        _minio.StatusCode = HttpStatusCode.ServiceUnavailable;
        using var client = _factory!.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_PostgresDown_ReturnsServiceUnavailableAsync()
    {
        // Arrange
        await _postgres.StopAsync();
        using var client = _factory!.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private sealed class StubMinioHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(StatusCode));
    }
}
