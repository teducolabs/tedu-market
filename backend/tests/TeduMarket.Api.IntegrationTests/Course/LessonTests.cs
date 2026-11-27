using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Api.IntegrationTests.Identity;
using TeduMarket.Domain.Course;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Api.IntegrationTests.Course;

/// <summary>Luồng: Instructor/Admin thêm, sửa, xóa, sắp xếp lại Bài học và đánh dấu Bài học xem thử, kiểm tra sở hữu khóa (T27, FR-20, FR-45, NFR-08).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class LessonTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
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
    public async Task Add_OwnerInstructor_AppendsLessonsInOrderAndListReturnsThemAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var first = await owner.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "  Giới thiệu " });
        var second = await owner.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "Cài đặt" });
        var list = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(["Giới thiệu", "Cài đặt"], list.EnumerateArray().Select(item => item.GetProperty("lessonTitle").GetString()));
        Assert.Equal([1, 2], list.EnumerateArray().Select(item => item.GetProperty("lessonOrder").GetInt32()));
        Assert.All(list.EnumerateArray(), item => Assert.False(item.GetProperty("isPreview").GetBoolean()));
    }

    [Fact]
    public async Task Add_AdminOnInstructorCourse_ReturnsCreatedAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        owner.Dispose();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "Admin thêm" });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(1, await CountLessonsAsync(courseId));
    }

    [Fact]
    public async Task Add_BlankTitle_ReturnsFieldErrorAndStoresNothingAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var response = await owner.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = " " });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("lessonTitle", out _));
        Assert.Equal(0, await CountLessonsAsync(courseId));
    }

    [Fact]
    public async Task Write_InstructorOfOtherCourse_ReturnsForbiddenAndKeepsLessonAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        var lessonId = await AddLessonAsync(owner, courseId, "Của chủ khóa");
        owner.Dispose();
        var attacker = await CreateInstructorAsync();
        using var client = await CreateAuthorizedClientAsync(attacker.UserId);

        // Act
        var add = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "Xâm nhập" });
        var update = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}", new { lessonTitle = "Chiếm" });
        var delete = await client.DeleteAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}");
        var reorder = await client.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/order", new { lessonIds = new[] { lessonId } });
        var list = await client.GetAsync($"/api/v1/courses/{courseId}/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, add.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, reorder.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        var stored = await FindLessonAsync(lessonId);
        Assert.Equal("Của chủ khóa", stored.LessonTitle);
        Assert.Equal(1, await CountLessonsAsync(courseId));
    }

    [Fact]
    public async Task Write_StudentAndNoToken_ReturnForbiddenAndUnauthorizedAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        owner.Dispose();
        using var student = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));
        using var anonymous = Host.CreateClient();

        // Act
        var asStudent = await student.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "A" });
        var asAnonymous = await anonymous.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle = "A" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, asStudent.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, asAnonymous.StatusCode);
        Assert.Equal(0, await CountLessonsAsync(courseId));
    }

    [Fact]
    public async Task Update_OwnerInstructor_ChangesTitleKeepsOrderAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        await AddLessonAsync(owner, courseId, "Bài 1");
        var lessonId = await AddLessonAsync(owner, courseId, "Bài 2");

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}", new { lessonTitle = " Bài hai " });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await FindLessonAsync(lessonId);
        Assert.Equal("Bài hai", stored.LessonTitle);
        Assert.Equal(2, stored.LessonOrder);
    }

    [Fact]
    public async Task Update_LessonOfAnotherCourseViaOwnCoursePath_ReturnsNotFoundAndKeepsTitleAsync()
    {
        // Arrange
        var (victimClient, victimCourseId) = await CreateOwnerWithCourseAsync();
        var victimLessonId = await AddLessonAsync(victimClient, victimCourseId, "Của nạn nhân");
        victimClient.Dispose();
        var (attackerClient, attackerCourseId) = await CreateOwnerWithCourseAsync();
        using var attacker = attackerClient;

        // Act
        var response = await attacker.PutAsJsonAsync($"/api/v1/courses/{attackerCourseId}/lessons/{victimLessonId}", new { lessonTitle = "Chiếm" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Của nạn nhân", (await FindLessonAsync(victimLessonId)).LessonTitle);
    }

    [Fact]
    public async Task Reorder_FullNewOrder_StoresAndListReturnsItAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var a = await AddLessonAsync(owner, courseId, "A");
        var b = await AddLessonAsync(owner, courseId, "B");
        var c = await AddLessonAsync(owner, courseId, "C");

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/order", new { lessonIds = new[] { c, a, b } });
        var list = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([c, a, b], list.EnumerateArray().Select(item => item.GetProperty("lessonId").GetGuid()));
        Assert.Equal([1, 2, 3], list.EnumerateArray().Select(item => item.GetProperty("lessonOrder").GetInt32()));
        Assert.Equal(1, (await FindLessonAsync(c)).LessonOrder);
    }

    [Fact]
    public async Task Reorder_MissingLesson_ReturnsFieldErrorAndKeepsOrderAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var a = await AddLessonAsync(owner, courseId, "A");
        await AddLessonAsync(owner, courseId, "B");

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/order", new { lessonIds = new[] { a } });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("lessonIds", out _));
        Assert.Equal(1, (await FindLessonAsync(a)).LessonOrder);
    }

    [Fact]
    public async Task SetPreview_OwnerInstructor_ListReturnsPreviewFlagAndUnmarkClearsItAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var marked = await AddLessonAsync(owner, courseId, "Xem thử");
        await AddLessonAsync(owner, courseId, "Bình thường");

        // Act
        var mark = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{marked}/preview", new { isPreview = true });
        var afterMark = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}/lessons");
        var unmark = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{marked}/preview", new { isPreview = false });

        // Assert
        Assert.Equal(HttpStatusCode.OK, mark.StatusCode);
        Assert.Equal([true, false], afterMark.EnumerateArray().Select(item => item.GetProperty("isPreview").GetBoolean()));
        Assert.Equal(HttpStatusCode.OK, unmark.StatusCode);
        Assert.False((await FindLessonAsync(marked)).IsPreview);
    }

    [Fact]
    public async Task SetPreview_AdminOnInstructorCourse_MarksLessonAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        var lessonId = await AddLessonAsync(owner, courseId, "Xem thử");
        owner.Dispose();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var response = await admin.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}/preview", new { isPreview = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await FindLessonAsync(lessonId)).IsPreview);
    }

    [Fact]
    public async Task SetPreview_InstructorOfOtherCourse_ReturnsForbiddenAndKeepsFlagAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        var lessonId = await AddLessonAsync(owner, courseId, "Bài 1");
        owner.Dispose();
        using var attacker = await CreateAuthorizedClientAsync((await CreateInstructorAsync()).UserId);

        // Act
        var response = await attacker.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}/preview", new { isPreview = true });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False((await FindLessonAsync(lessonId)).IsPreview);
    }

    [Fact]
    public async Task SetPreview_MissingFlag_ReturnsFieldErrorAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var lessonId = await AddLessonAsync(owner, courseId, "Bài 1");

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/lessons/{lessonId}/preview", new { });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("isPreview", out _));
    }

    [Fact]
    public async Task Delete_MiddleLesson_RemovesItAndCompactsOrderAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var a = await AddLessonAsync(owner, courseId, "A");
        var b = await AddLessonAsync(owner, courseId, "B");
        var c = await AddLessonAsync(owner, courseId, "C");

        // Act
        var response = await owner.DeleteAsync($"/api/v1/courses/{courseId}/lessons/{b}");
        var list = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/courses/{courseId}/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal([a, c], list.EnumerateArray().Select(item => item.GetProperty("lessonId").GetGuid()));
        Assert.Equal([1, 2], list.EnumerateArray().Select(item => item.GetProperty("lessonOrder").GetInt32()));
    }

    [Fact]
    public async Task Delete_UnknownLesson_ReturnsNotFoundAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var response = await owner.DeleteAsync($"/api/v1/courses/{courseId}/lessons/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCourse_WithLessons_RemovesLessonsTooAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        await AddLessonAsync(owner, courseId, "A");
        await AddLessonAsync(owner, courseId, "B");

        // Act
        var response = await owner.DeleteAsync($"/api/v1/courses/{courseId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await CountLessonsAsync(courseId));
    }

    private async Task<(HttpClient Client, Guid CourseId)> CreateOwnerWithCourseAsync()
    {
        var instructor = await CreateInstructorAsync();
        var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var categoryResponse = await CreateCategoryAsync();
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId = categoryResponse, title = "Khóa mẫu", price = 100000 });
        response.EnsureSuccessStatusCode();
        return (client, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("courseId").GetGuid());
    }

    private static async Task<Guid> AddLessonAsync(HttpClient client, Guid courseId, string lessonTitle)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/lessons", new { lessonTitle });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("lessonId").GetGuid();
    }

    private async Task<Guid> CreateCategoryAsync()
    {
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var response = await admin.PostAsJsonAsync("/api/v1/categories", new { name = $"Lập trình {Guid.NewGuid():N}" });
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

    private async Task<Lesson> FindLessonAsync(Guid lessonId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<Lesson>().Single(lesson => lesson.LessonId == lessonId);
    }

    private async Task<int> CountLessonsAsync(Guid courseId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<Lesson>().Count(lesson => lesson.CourseId == courseId);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }
}
