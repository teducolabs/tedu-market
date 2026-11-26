using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;
using TeduMarket.Infrastructure.Identity;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: Admin sửa/xóa Hồ sơ Instructor, Instructor sửa hồ sơ của mình (T22, FR-18, FR-43).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class InstructorProfileTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Update_OwnerInstructor_ReturnsUpdatedProfileAndReadBackMatchesAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}", new
        {
            displayName = "  Nguyễn Văn A ",
            biography = "Giảng dạy 10 năm",
            avatarUrl = "https://cdn.example.test/a.png",
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var readBack = await client.GetFromJsonAsync<JsonElement>($"/api/v1/instructor-profiles/{instructor.ProfileId}");
        Assert.Equal("Nguyễn Văn A", readBack.GetProperty("displayName").GetString());
        Assert.Equal("Giảng dạy 10 năm", readBack.GetProperty("biography").GetString());
        Assert.Equal("https://cdn.example.test/a.png", readBack.GetProperty("avatarUrl").GetString());
    }

    [Fact]
    public async Task Update_AdminEditsInstructorProfile_ChangeVisibleToOwnerAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PutAsJsonAsync(
            $"/api/v1/instructor-profiles/{instructor.ProfileId}", new { displayName = "Tên do Admin sửa" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var owner = await CreateAuthorizedClientAsync(instructor.UserId);
        var mine = await owner.GetFromJsonAsync<JsonElement>("/api/v1/instructor-profiles/me");
        Assert.Equal("Tên do Admin sửa", mine.GetProperty("displayName").GetString());
        Assert.Equal(instructor.ProfileId, mine.GetProperty("instructorProfileId").GetGuid());
    }

    [Fact]
    public async Task Update_InstructorEditsOtherInstructorProfile_ReturnsForbiddenAndKeepsProfileAsync()
    {
        // Arrange
        var victim = await CreateInstructorAsync();
        var attacker = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(attacker.UserId);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/instructor-profiles/{victim.ProfileId}", new { displayName = "Chiếm hồ sơ" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(await GetDisplayNameAsync(victim.ProfileId));
    }

    [Fact]
    public async Task Update_InvalidContent_ReturnsFieldErrorsAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/v1/instructor-profiles/{instructor.ProfileId}", new { displayName = " ", avatarUrl = "javascript:alert(1)" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("displayName", out _));
        Assert.True(errors.TryGetProperty("avatarUrl", out _));
    }

    [Fact]
    public async Task Update_UnknownProfile_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/instructor-profiles/{Guid.NewGuid()}", new { displayName = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/instructor-profiles/{Guid.NewGuid()}", new { displayName = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_Student_ReturnsForbiddenAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var student = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));

        // Act
        var response = await student.PutAsJsonAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}", new { displayName = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_InstructorReadsOtherInstructorProfile_ReturnsForbiddenAsync()
    {
        // Arrange
        var victim = await CreateInstructorAsync();
        var other = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(other.UserId);

        // Act
        var response = await client.GetAsync($"/api/v1/instructor-profiles/{victim.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_Student_ReturnsForbiddenAsync()
    {
        // Arrange
        using var student = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));

        // Act
        var response = await student.GetAsync("/api/v1/instructor-profiles/me");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AdminDeletesProfile_RemovesProfileAndInstructorRoleAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountProfilesAsync(instructor.UserId));
        Assert.Equal([RoleNames.Student], await GetRolesAsync(instructor.UserId));
        using var signedInAgain = await CreateAuthorizedClientAsync(instructor.UserId);
        Assert.Equal(HttpStatusCode.Forbidden, (await signedInAgain.GetAsync("/api/v1/samples/instructor-only")).StatusCode);
    }

    [Fact]
    public async Task Delete_AdminDeletesProfile_RevokesTokensOfDeletedInstructorAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var oldSession = await CreateAuthorizedClientAsync(instructor.UserId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        await admin.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Act
        var response = await oldSession.GetAsync("/api/v1/samples/instructor-only");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownProfile_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/instructor-profiles/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/instructor-profiles/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_InstructorDeletesOwnProfile_ReturnsForbiddenAndKeepsProfileAndRoleAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, await CountProfilesAsync(instructor.UserId));
        Assert.Equal([RoleNames.Instructor, RoleNames.Student], await GetRolesAsync(instructor.UserId));
    }

    private async Task<(Guid UserId, Guid ProfileId)> CreateInstructorAsync()
    {
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var response = await admin.PostAsync($"/api/v1/users/{userId}/instructor-approval", content: null);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (userId, body.GetProperty("instructorProfileId").GetGuid());
    }

    private async Task<string?> GetDisplayNameAsync(Guid profileId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<InstructorProfile>().Single(profile => profile.InstructorProfileId == profileId).DisplayName;
    }

    private async Task<int> CountProfilesAsync(Guid userId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<InstructorProfile>().Count(profile => profile.UserId == userId);
    }

    private async Task<string[]> GetRolesAsync(Guid userId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        return [.. (await userManager.GetRolesAsync(user!)).Order()];
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
