using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Luồng: Admin duyệt Instructor (T21, FR-40) và xem danh sách người dùng.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class InstructorApprovalTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Approve_StudentByAdmin_ReturnsProfileAndRolesAreAssignedAsync()
    {
        // Arrange
        var studentId = await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(studentId, body.GetProperty("userId").GetGuid());
        Assert.NotEqual(Guid.Empty, body.GetProperty("instructorProfileId").GetGuid());
        Assert.Equal([RoleNames.Instructor, RoleNames.Student], await GetRolesAsync(studentId));
    }

    [Fact]
    public async Task Approve_StudentByAdmin_NewSignInGetsInstructorAccessAsync()
    {
        // Arrange
        var studentId = await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        await admin.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Act
        using var instructor = await CreateAuthorizedClientAsync(studentId);
        var response = await instructor.GetAsync("/api/v1/samples/instructor-only");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Approve_StudentByAdmin_RevokesOldTokensSoStudentMustSignInAgainAsync()
    {
        // Arrange
        var studentId = await Host.CreateUserAsync(RoleNames.Student);
        using var oldSession = await CreateAuthorizedClientAsync(studentId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        await admin.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Act
        var response = await oldSession.GetAsync("/api/v1/samples/student-only");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_SameUserTwice_ReturnsBadRequestAndKeepsSingleProfileAsync()
    {
        // Arrange
        var studentId = await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        await admin.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Act
        var response = await admin.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INSTRUCTOR_ALREADY_APPROVED", body.GetProperty("errorCode").GetString());
        Assert.Equal(1, await CountProfilesAsync(studentId));
    }

    [Fact]
    public async Task Approve_UnknownUser_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsync($"/api/v1/users/{Guid.NewGuid()}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AdminAccount_ReturnsBadRequestAsync()
    {
        // Arrange
        var otherAdminId = await Host.CreateUserAsync(RoleNames.Admin);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsync($"/api/v1/users/{otherAdminId}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("USER_NOT_STUDENT", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Approve_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/v1/users/{Guid.NewGuid()}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    public async Task Approve_NotAdmin_ReturnsForbiddenAndAssignsNothingAsync(string role)
    {
        // Arrange
        var studentId = await Host.CreateUserAsync(RoleNames.Student);
        using var caller = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(role));

        // Act
        var response = await caller.PostAsync($"/api/v1/users/{studentId}/instructor-approval", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal([RoleNames.Student], await GetRolesAsync(studentId));
        Assert.Equal(0, await CountProfilesAsync(studentId));
    }

    [Fact]
    public async Task ListUsers_AdminFiltersByRole_ReturnsOnlyThatRoleWithProfileIdAsync()
    {
        // Arrange
        var instructorId = await Host.CreateUserAsync(RoleNames.Student);
        await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        await admin.PostAsync($"/api/v1/users/{instructorId}/instructor-approval", content: null);

        // Act
        var response = await admin.GetAsync("/api/v1/users?role=Instructor");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        var item = Assert.Single(body.GetProperty("items").EnumerateArray());
        Assert.Equal(instructorId, item.GetProperty("userId").GetGuid());
        Assert.NotEqual(JsonValueKind.Null, item.GetProperty("instructorProfileId").ValueKind);
    }

    [Fact]
    public async Task ListUsers_AdminPagesWithoutFilter_ReturnsRequestedPageAndTotalAsync()
    {
        // Arrange
        await Host.CreateUserAsync(RoleNames.Student);
        await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.GetAsync("/api/v1/users?pageNumber=2&pageSize=2");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("items").GetArrayLength());
        Assert.Equal(2, body.GetProperty("pageNumber").GetInt32());
    }

    [Fact]
    public async Task ListUsers_UnknownRole_ReturnsFieldErrorAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.GetAsync("/api/v1/users?role=Teacher");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors").TryGetProperty("role", out _));
    }

    [Fact]
    public async Task ListUsers_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleNames.Student)]
    [InlineData(RoleNames.Instructor)]
    public async Task ListUsers_NotAdmin_ReturnsForbiddenAsync(string role)
    {
        // Arrange
        using var caller = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(role));

        // Act
        var response = await caller.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string[]> GetRolesAsync(Guid userId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        return [.. (await userManager.GetRolesAsync(user!)).Order()];
    }

    private async Task<int> CountProfilesAsync(Guid userId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TeduMarket.Infrastructure.Persistence.AppDbContext>();
        return dbContext.Set<TeduMarket.Domain.Instructor.InstructorProfile>().Count(profile => profile.UserId == userId);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
