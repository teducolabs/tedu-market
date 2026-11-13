using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;

namespace TeduMarket.Api.IntegrationTests.Common;

public sealed class CorrelationIdTests : IDisposable
{
    private readonly CollectingLogSink _sink = new();
    private readonly WebApplicationFactory<Program> _factory;

    public CorrelationIdTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<ILogEventSink>(_sink)));
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetSampleFailure_WithCorrelationIdHeader_ReturnsSameIdInHeaderProblemAndAllLogsAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/samples/failure");
        request.Headers.Add("X-Correlation-Id", "abc-123");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal("abc-123", response.Headers.GetValues("X-Correlation-Id").Single());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("abc-123", json.RootElement.GetProperty("correlationId").GetString());
        var requestLogs = _sink.Events.Where(e => e.Properties.TryGetValue("CorrelationId", out var value) && value.ToString() == "\"abc-123\"").ToList();
        Assert.Contains(requestLogs, e => e.Exception is not null);
        Assert.Contains(requestLogs, e => e.MessageTemplate.Text.Contains("HTTP", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetUnknownPath_WithoutHeader_ReturnsGeneratedIdInHeaderAndProblemAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/khong-ton-tai");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var headerId = response.Headers.GetValues("X-Correlation-Id").Single();
        Assert.NotEmpty(headerId);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(headerId, json.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task GetSampleFailure_WithInvalidCorrelationIdHeader_ReplacesItWithGeneratedIdAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/samples/failure");
        request.Headers.Add("X-Correlation-Id", "bad id\"{x}");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        var headerId = response.Headers.GetValues("X-Correlation-Id").Single();
        Assert.NotEqual("bad id\"{x}", headerId);
        Assert.NotEmpty(headerId);
    }

    [Fact]
    public async Task PostSample_WithSecretsInHeadersAndBody_DoesNotWriteSecretsToLogsAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/samples?token=query-secret")
        {
            Content = JsonContent.Create(new { title = "" }),
        };
        request.Headers.Add("Authorization", "Bearer bearer-secret");
        request.Headers.Add("X-API-Token", "hook-secret");

        // Act
        await client.SendAsync(request);

        // Assert
        var rendered = string.Join('\n', _sink.Events.Select(e => e.RenderMessage(CultureInfo.InvariantCulture) + string.Join(',', e.Properties.Values)));
        Assert.DoesNotContain("bearer-secret", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("hook-secret", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("query-secret", rendered, StringComparison.Ordinal);
    }
}
