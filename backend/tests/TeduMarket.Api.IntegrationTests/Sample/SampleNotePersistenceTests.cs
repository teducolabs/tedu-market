using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TeduMarket.Api.IntegrationTests.Common;

namespace TeduMarket.Api.IntegrationTests.Sample;

[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class SampleNotePersistenceTests(PostgresFixture postgres) : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;

    public async Task InitializeAsync()
    {
        var connectionString = await postgres.CreateDatabaseAsync();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Default", connectionString));
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetSampleNotes_AfterSaveByEfCore_ReturnsNoteReadByDapper_Regression_T03Async()
    {
        // Arrange
        using var client = _factory!.CreateClient();
        var saveResponse = await client.PostAsJsonAsync("/api/v1/samples/notes", new { title = "Ghi chú mẫu" });
        saveResponse.EnsureSuccessStatusCode();

        // Act
        var response = await client.GetAsync("/api/v1/samples/notes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var notes = json.RootElement.EnumerateArray().ToList();
        Assert.Single(notes);
        Assert.Equal("Ghi chú mẫu", notes[0].GetProperty("title").GetString());
    }
}
