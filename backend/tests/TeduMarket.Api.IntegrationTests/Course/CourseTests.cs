using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Api.IntegrationTests.Identity;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;
using TeduMarket.Domain.Instructor;
using TeduMarket.Infrastructure.Identity;
using TeduMarket.Infrastructure.Persistence;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Api.IntegrationTests.Course;

/// <summary>Luồng: Instructor/Admin tạo và sửa Khóa học ở trạng thái Chưa đăng, kiểm tra sở hữu, chặn xóa khi còn khóa (T26, FR-19, FR-22, NFR-08).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class CourseTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Create_Instructor_ReturnsDraftOwnedByOwnProfileAndPersistsItAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "  C# cơ bản ", description = "Mô tả", price = 499000 });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Draft", body.GetProperty("status").GetString());
        Assert.Equal(instructor.ProfileId, body.GetProperty("instructorProfileId").GetGuid());
        var stored = await FindCourseAsync(body.GetProperty("courseId").GetGuid());
        Assert.Equal(CourseStatus.Draft, stored.Status);
        Assert.Equal("C# cơ bản", stored.Title);
        Assert.Equal(499_000m, stored.Price);
        Assert.Equal(categoryId, stored.CategoryId);
        Assert.Equal(instructor.ProfileId, stored.InstructorProfileId);
    }

    [Fact]
    public async Task Create_AdminForInstructor_StoresCourseUnderThatInstructorAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Thiết kế");
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsJsonAsync(
            "/api/v1/courses", new { instructorProfileId = instructor.ProfileId, categoryId, title = "Figma", price = 0 });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var stored = await FindCourseAsync((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("courseId").GetGuid());
        Assert.Equal(instructor.ProfileId, stored.InstructorProfileId);
        Assert.Equal(CourseStatus.Draft, stored.Status);
    }

    [Fact]
    public async Task Create_AdminWithoutInstructorProfileId_ReturnsFieldErrorAsync()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "A", price = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("instructorProfileId", out _));
    }

    [Fact]
    public async Task Create_InstructorForOtherInstructorProfile_ReturnsForbiddenAndStoresNothingAsync()
    {
        // Arrange
        var victim = await CreateInstructorAsync();
        var attacker = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(attacker.UserId);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/courses", new { instructorProfileId = victim.ProfileId, categoryId, title = "Giả mạo", price = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, await CountCoursesAsync());
    }

    [Fact]
    public async Task Create_InvalidInput_ReturnsFieldErrorsAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId = Guid.Empty, title = " ", price = -5 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("categoryId", out _));
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("price", out _));
        Assert.Equal(0, await CountCoursesAsync());
    }

    [Fact]
    public async Task Create_UnknownCategory_ReturnsFieldErrorOnCategoryIdAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId = Guid.NewGuid(), title = "A", price = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("categoryId", out _));
    }

    [Fact]
    public async Task Create_TwoCoursesWithSameTitle_GetDistinctSlugsFromTitleAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);

        // Act
        var first = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "Lập trình C# cơ bản", price = 1 });
        var second = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "lập trình c# CƠ BẢN", price = 1 });

        // Assert
        var firstSlug = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("slug").GetString();
        var secondSlug = (await second.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("slug").GetString();
        Assert.Equal("lap-trinh-c-co-ban", firstSlug);
        Assert.Equal("lap-trinh-c-co-ban-2", secondSlug);
    }

    [Fact]
    public async Task Update_NewTitle_KeepsSlugAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var courseId = await CreateCourseAsync(client, categoryId);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}", new { categoryId, title = "Tên khác hẳn", price = 1 });

        // Assert
        Assert.Equal("khoa-mau", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("slug").GetString());
        Assert.Equal("khoa-mau", (await FindCourseAsync(courseId)).Slug);
    }

    [Fact]
    public async Task Delete_OwnerInstructor_ReturnsNoContentAndRemovesCourseAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var courseId = await CreateCourseAsync(client, categoryId);

        // Act
        var response = await client.DeleteAsync($"/api/v1/courses/{courseId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountCoursesAsync());
    }

    [Fact]
    public async Task Delete_AdminOnInstructorCourse_ReturnsNoContentAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var courseId = await CreateCourseAsync(client, categoryId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/courses/{courseId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountCoursesAsync());
    }

    [Fact]
    public async Task Delete_InstructorOfOtherCourse_ReturnsForbiddenAndKeepsCourseAsync()
    {
        // Arrange
        var owner = await CreateInstructorAsync();
        var attacker = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var ownerClient = await CreateAuthorizedClientAsync(owner.UserId);
        var courseId = await CreateCourseAsync(ownerClient, categoryId);
        using var attackerClient = await CreateAuthorizedClientAsync(attacker.UserId);

        // Act
        var response = await attackerClient.DeleteAsync($"/api/v1/courses/{courseId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, await CountCoursesAsync());
    }

    [Fact]
    public async Task Delete_UnknownCourse_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/courses/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_StudentAndNoToken_ReturnForbiddenAndUnauthorizedAsync()
    {
        // Arrange
        using var student = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));
        using var guest = Host.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var asStudent = await student.DeleteAsync($"/api/v1/courses/{id}");
        var asGuest = await guest.DeleteAsync($"/api/v1/courses/{id}");

        // Assert
        Assert.Equal([HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized], [asStudent.StatusCode, asGuest.StatusCode]);
    }

    [Fact]
    public async Task DeleteInstructorProfile_AfterCourseDeleted_SucceedsAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var courseId = await CreateCourseAsync(client, categoryId);
        await client.DeleteAsync($"/api/v1/courses/{courseId}");
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_OwnerInstructor_ReplacesInfoAndKeepsDraftAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var oldCategoryId = await CreateCategoryAsync("Cũ");
        var newCategoryId = await CreateCategoryAsync("Mới");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var courseId = await CreateCourseAsync(client, oldCategoryId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/v1/courses/{courseId}", new { categoryId = newCategoryId, title = "Tiêu đề mới", description = (string?)null, price = 750000 });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await FindCourseAsync(courseId);
        Assert.Equal("Tiêu đề mới", stored.Title);
        Assert.Null(stored.Description);
        Assert.Equal(750_000m, stored.Price);
        Assert.Equal(newCategoryId, stored.CategoryId);
        Assert.Equal(instructor.ProfileId, stored.InstructorProfileId);
        Assert.Equal(CourseStatus.Draft, stored.Status);
        Assert.NotNull(stored.UpdatedAt);
    }

    [Fact]
    public async Task Update_InstructorOfOtherCourse_ReturnsForbiddenAndKeepsCourseAsync()
    {
        // Arrange
        var owner = await CreateInstructorAsync();
        var attacker = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var ownerClient = await CreateAuthorizedClientAsync(owner.UserId);
        var courseId = await CreateCourseAsync(ownerClient, categoryId);
        using var attackerClient = await CreateAuthorizedClientAsync(attacker.UserId);

        // Act
        var response = await attackerClient.PutAsJsonAsync($"/api/v1/courses/{courseId}", new { categoryId, title = "Bị chiếm", price = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var stored = await FindCourseAsync(courseId);
        Assert.Equal("Khóa mẫu", stored.Title);
        Assert.Equal(owner.ProfileId, stored.InstructorProfileId);
    }

    [Fact]
    public async Task Update_AdminOnInstructorCourse_UpdatesWithoutChangingOwnerAsync()
    {
        // Arrange
        var owner = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var ownerClient = await CreateAuthorizedClientAsync(owner.UserId);
        var courseId = await CreateCourseAsync(ownerClient, categoryId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/courses/{courseId}", new { categoryId, title = "Admin sửa", price = 2 });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await FindCourseAsync(courseId);
        Assert.Equal("Admin sửa", stored.Title);
        Assert.Equal(owner.ProfileId, stored.InstructorProfileId);
    }

    [Fact]
    public async Task Update_UnknownCourse_ReturnsNotFoundAsync()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/courses/{Guid.NewGuid()}", new { categoryId, title = "A", price = 1 });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Write_Student_ReturnsForbiddenAndStoresNothingAsync()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));

        // Act
        var create = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "A", price = 1 });
        var update = await client.PutAsJsonAsync($"/api/v1/courses/{Guid.NewGuid()}", new { categoryId, title = "A", price = 1 });

        // Assert
        Assert.Equal([HttpStatusCode.Forbidden, HttpStatusCode.Forbidden], [create.StatusCode, update.StatusCode]);
        Assert.Equal(0, await CountCoursesAsync());
    }

    [Fact]
    public async Task Write_NoToken_ReturnsUnauthorizedAsync()
    {
        // Arrange
        using var client = Host.CreateClient();
        var body = new { categoryId = Guid.NewGuid(), title = "A", price = 1 };

        // Act
        var create = await client.PostAsJsonAsync("/api/v1/courses", body);
        var update = await client.PutAsJsonAsync($"/api/v1/courses/{Guid.NewGuid()}", body);

        // Assert
        Assert.Equal([HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized], [create.StatusCode, update.StatusCode]);
    }

    [Fact]
    public async Task DeleteCategory_CategoryHasCourse_ReturnsCategoryInUseAndKeepsCategoryAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Đang dùng");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        await CreateCourseAsync(client, categoryId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CATEGORY_IN_USE", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errorCode").GetString());
        var list = await Host.CreateClient().GetFromJsonAsync<JsonElement[]>("/api/v1/categories");
        Assert.Equal([categoryId], list!.Select(item => item.GetProperty("categoryId").GetGuid()));
    }

    [Fact]
    public async Task DeleteInstructorProfile_InstructorHasCourse_ReturnsInstructorHasCoursesAndKeepsProfileAndRoleAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        var categoryId = await CreateCategoryAsync("Lập trình");
        using var client = await CreateAuthorizedClientAsync(instructor.UserId);
        await CreateCourseAsync(client, categoryId);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INSTRUCTOR_HAS_COURSES", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errorCode").GetString());
        Assert.True(await ProfileExistsAsync(instructor.ProfileId));
        Assert.Contains(RoleNames.Instructor, await GetRolesAsync(instructor.UserId));
    }

    [Fact]
    public async Task DeleteInstructorProfile_InstructorHasNoCourse_StillSucceedsAsync()
    {
        // Arrange
        var instructor = await CreateInstructorAsync();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.DeleteAsync($"/api/v1/instructor-profiles/{instructor.ProfileId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(await ProfileExistsAsync(instructor.ProfileId));
        Assert.DoesNotContain(RoleNames.Instructor, await GetRolesAsync(instructor.UserId));
    }

    private static async Task<Guid> CreateCourseAsync(HttpClient client, Guid categoryId)
    {
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "Khóa mẫu", price = 100000 });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("courseId").GetGuid();
    }

    private async Task<Guid> CreateCategoryAsync(string name)
    {
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("categoryId").GetGuid();
    }

    private async Task<(Guid UserId, Guid ProfileId)> CreateInstructorAsync()
    {
        var userId = await Host.CreateUserAsync(RoleNames.Student);
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var response = await admin.PostAsync($"/api/v1/users/{userId}/instructor-approval", content: null);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (userId, body.GetProperty("instructorProfileId").GetGuid());
    }

    private async Task<CourseAggregate> FindCourseAsync(Guid courseId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<CourseAggregate>().Single(course => course.CourseId == courseId);
    }

    private async Task<int> CountCoursesAsync()
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<CourseAggregate>().Count();
    }

    private async Task<bool> ProfileExistsAsync(Guid profileId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<InstructorProfile>().Any(profile => profile.InstructorProfileId == profileId);
    }

    private async Task<string[]> GetRolesAsync(Guid userId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        return [.. await userManager.GetRolesAsync(user!)];
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
