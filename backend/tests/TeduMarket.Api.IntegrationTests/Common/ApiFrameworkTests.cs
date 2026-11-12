using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TeduMarket.Api.IntegrationTests.Common;

public sealed class ApiFrameworkTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostSample_ValidTitle_ReturnsOkAsync()
    {
        // Arrange
        var body = new { title = "Tiêu đề" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/samples", body);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Tiêu đề", json.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task PostSample_EmptyTitle_ReturnsBadRequestWithFieldErrorsAsync()
    {
        // Arrange
        var body = new { title = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/samples", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("INVALID_REQUEST", json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Không được để trống", json.RootElement.GetProperty("errors").GetProperty("title")[0].GetString());
        Assert.True(json.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task GetSampleFailure_UnexpectedException_ReturnsProblemDetailsWithoutLeakAsync()
    {
        // Arrange
        const string url = "/api/v1/samples/failure";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        Assert.Equal("INTERNAL_ERROR", json.RootElement.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("Lỗi mẫu không lường trước", content, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", content, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetOpenApiDocument_Always_ReturnsOkWithApiV1PathsAsync()
    {
        // Arrange
        const string url = "/openapi/v1.json";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("paths").TryGetProperty("/api/v1/samples", out _));
    }
}
