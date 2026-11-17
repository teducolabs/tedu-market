using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Infrastructure.Health;

namespace TeduMarket.Api.IntegrationTests.Health;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class HealthEndpointsTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private readonly StubMinioHandler _minio = new();
    private WebApplicationFactory<Program>? _factory;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        _connectionString = await postgres.CreateDatabaseAsync();
        _factory = CreateFactory(_connectionString);
    }

    public void Dispose() => _minio.Dispose();

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetHealthLive_DependenciesDown_ReturnsOkAsync()
    {
        // Arrange
        _minio.StatusCode = HttpStatusCode.ServiceUnavailable;
        using var downFactory = CreateFactory(UnreachableConnectionString());
        using var client = downFactory.CreateClient();

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
        using var downFactory = CreateFactory(UnreachableConnectionString());
        using var client = downFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory(string connectionString) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", connectionString);
            builder.ConfigureServices(services =>
                services.AddHttpClient(MinioHealthCheck.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => _minio));
        });

    /// <summary>Trỏ tới database không tồn tại trong cùng container, mô phỏng PostgreSQL không dùng được mà không dừng container dùng chung.</summary>
    private string UnreachableConnectionString() =>
        new NpgsqlConnectionStringBuilder(_connectionString) { Database = "database_not_exist", Timeout = 3 }.ConnectionString;

    private sealed class StubMinioHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(StatusCode));
    }
}
