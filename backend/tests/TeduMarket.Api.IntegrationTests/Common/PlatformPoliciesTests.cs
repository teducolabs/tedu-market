using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.Common;

namespace TeduMarket.Api.IntegrationTests.Common;

[Trait("Category", "Integration")]
public sealed class PlatformPoliciesTests : IDisposable
{
    private const string _allowedOrigin = "http://localhost:3000";
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetSample_AllowedOrigin_ReturnsAllowOriginHeaderAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/samples/notes");
        request.Headers.Add("Origin", _allowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(_allowedOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task GetSample_UnknownOrigin_ReturnsNoAllowOriginHeaderAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/samples/limited");
        request.Headers.Add("Origin", "https://evil.example");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_UnknownOrigin_ReturnsNoAllowOriginHeaderAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/samples");
        request.Headers.Add("Origin", "https://evil.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task GetHealthLive_Always_ReturnsSecurityHeadersAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
        Assert.True(response.Headers.Contains("Permissions-Policy"));
    }

    [Fact]
    public async Task GetLimited_ElevenRequestsInOneMinute_ReturnsTooManyRequestsOnEleventhAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var statuses = new List<HttpStatusCode>();

        // Act
        for (var i = 0; i < ApiPolicies.CheckoutPermitLimit + 1; i++)
        {
            statuses.Add((await client.GetAsync("/api/v1/samples/limited")).StatusCode);
        }

        // Assert
        Assert.All(statuses.Take(ApiPolicies.CheckoutPermitLimit), status => Assert.Equal(HttpStatusCode.OK, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[^1]);
    }

    [Fact]
    public async Task GetLimited_TooManyRequests_ReturnsProblemDetailsWithRetryAfterAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        for (var i = 0; i < ApiPolicies.CheckoutPermitLimit; i++)
        {
            await client.GetAsync("/api/v1/samples/limited");
        }

        // Act
        var response = await client.GetAsync("/api/v1/samples/limited");

        // Assert
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.Contains("Retry-After"));
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("RATE_LIMITED", json.RootElement.GetProperty("errorCode").GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task GetCached_RepeatedRequest_ReturnsCachedBodyAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var first = await client.GetStringAsync("/api/v1/samples/cached");
        var second = await client.GetStringAsync("/api/v1/samples/cached");

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task GetCached_AfterEvictByTag_ReturnsFreshBodyAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var cached = await client.GetStringAsync("/api/v1/samples/cached");
        var store = _factory.Services.GetRequiredService<IOutputCacheStore>();

        // Act
        await store.EvictByTagAsync(ApiPolicies.CatalogCacheTag, CancellationToken.None);
        var fresh = await client.GetStringAsync("/api/v1/samples/cached");

        // Assert
        Assert.NotEqual(cached, fresh);
    }
}
