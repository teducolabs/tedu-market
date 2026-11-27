using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Api.IntegrationTests.Identity;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Api.IntegrationTests.Course;

/// <summary>Luồng: Admin quản lý Category, danh sách công khai phản ánh thay đổi (T25, FR-17).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class CategoryTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Create_Admin_ReturnsCreatedAndPublicListShowsItWithoutTokenAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        using var guest = Host.CreateClient();

        // Act
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name = "  Lập trình web " });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Lập trình web", created.GetProperty("name").GetString());
        var list = await guest.GetFromJsonAsync<JsonElement[]>("/api/v1/categories");
        Assert.Equal([created.GetProperty("categoryId").GetGuid()], list!.Select(item => item.GetProperty("categoryId").GetGuid()));
    }

    [Fact]
    public async Task Update_Admin_RenamesAndPublicListReflectsChangeImmediatelyAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        using var guest = Host.CreateClient();
        var categoryId = await CreateCategoryAsync(admin, "Cũ");
        await guest.GetFromJsonAsync<JsonElement[]>("/api/v1/categories");

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/categories/{categoryId}", new { name = "Mới" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await guest.GetFromJsonAsync<JsonElement[]>("/api/v1/categories");
        Assert.Equal(["Mới"], list!.Select(item => item.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Delete_Admin_ReturnsNoContentAndPublicListNoLongerShowsItAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        using var guest = Host.CreateClient();
        var categoryId = await CreateCategoryAsync(admin, "Xóa tôi");
        await guest.GetFromJsonAsync<JsonElement[]>("/api/v1/categories");

        // Act
        var response = await admin.DeleteAsync($"/api/v1/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await guest.GetFromJsonAsync<JsonElement[]>("/api/v1/categories"))!);
    }

    [Fact]
    public async Task List_ManyCategories_ReturnsThemSortedByNameAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        await CreateCategoryAsync(admin, "Thiết kế");
        await CreateCategoryAsync(admin, "An toàn thông tin");

        // Act
        var list = await Host.CreateClient().GetFromJsonAsync<JsonElement[]>("/api/v1/categories");

        // Assert
        Assert.Equal(["An toàn thông tin", "Thiết kế"], list!.Select(item => item.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Create_BlankName_ReturnsFieldErrorOnNameAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);

        // Act
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name = " " });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("name", out _));
    }

    [Fact]
    public async Task Create_DuplicateNameIgnoringCase_ReturnsFieldErrorOnNameAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        await CreateCategoryAsync(admin, "Lập trình");

        // Act
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name = "LẬP TRÌNH" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("name", out _));
        Assert.Single((await Host.CreateClient().GetFromJsonAsync<JsonElement[]>("/api/v1/categories"))!);
    }

    [Fact]
    public async Task Update_UnknownCategory_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/categories/{Guid.NewGuid()}", new { name = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownCategory_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);

        // Act
        var response = await admin.DeleteAsync($"/api/v1/categories/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleNames.Instructor)]
    [InlineData(RoleNames.Student)]
    public async Task Write_NotAdmin_ReturnsForbiddenAndKeepsCategoryAsync(string role)
    {
        // Arrange
        using var admin = await CreateClientAsync(RoleNames.Admin);
        var categoryId = await CreateCategoryAsync(admin, "Giữ nguyên");
        using var client = await CreateClientAsync(role);

        // Act
        var create = await client.PostAsJsonAsync("/api/v1/categories", new { name = "Mới" });
        var update = await client.PutAsJsonAsync($"/api/v1/categories/{categoryId}", new { name = "Đổi" });
        var delete = await client.DeleteAsync($"/api/v1/categories/{categoryId}");

        // Assert
        Assert.Equal(
            [HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden],
            [create.StatusCode, update.StatusCode, delete.StatusCode]);
        var list = await Host.CreateClient().GetFromJsonAsync<JsonElement[]>("/api/v1/categories");
        Assert.Equal(["Giữ nguyên"], list!.Select(item => item.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Write_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var create = await client.PostAsJsonAsync("/api/v1/categories", new { name = "A" });
        var update = await client.PutAsJsonAsync($"/api/v1/categories/{id}", new { name = "A" });
        var delete = await client.DeleteAsync($"/api/v1/categories/{id}");

        // Assert
        Assert.Equal(
            [HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized],
            [create.StatusCode, update.StatusCode, delete.StatusCode]);
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient admin, string name)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("categoryId").GetGuid();
    }

    private async Task<HttpClient> CreateClientAsync(string role)
    {
        var tokens = await Host.LoginAsync(await Host.CreateUserAsync(role));
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
