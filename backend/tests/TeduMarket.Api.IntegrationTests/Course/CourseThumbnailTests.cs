using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Api.IntegrationTests.Identity;
using TeduMarket.Domain.Identity;
using TeduMarket.Infrastructure.Persistence;
using CourseAggregate = TeduMarket.Domain.Course.Course;

namespace TeduMarket.Api.IntegrationTests.Course;

/// <summary>Luồng: Instructor/Admin xin url PUT, tải thumbnail thẳng lên MinIO rồi gắn vào Khóa học; file sai kiểu/dung lượng bị từ chối (T28, FR-23, NFR-08).</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class CourseThumbnailTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private readonly FakeMinioBackchannel _minio = new();
    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync()
    {
        _host = new IdentityTestHost(await postgres.CreateDatabaseAsync(), minio: _minio);
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
    public async Task Thumbnail_OwnerUploadsWithIssuedUrlAndSetsKey_CourseShowsThumbnailUrlAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var upload = await RequestUploadAsync(owner, courseId, "image/png", 1024);
        var put = await PutFileAsync(upload, "image/png", 1024);
        var set = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail", new { objectKey = upload.ObjectKey });

        // Assert
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);
        var body = await set.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"http://localhost:9000/thumbnails/{upload.ObjectKey}", body.GetProperty("thumbnailUrl").GetString());
        Assert.Equal(upload.ObjectKey, (await FindCourseAsync(courseId)).ThumbnailKey);
    }

    [Fact]
    public async Task Thumbnail_UploadedAgain_ReplacesPreviousKeyAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var first = await UploadAndSetAsync(owner, courseId, "image/png");

        // Act
        var second = await UploadAndSetAsync(owner, courseId, "image/webp");

        // Assert
        Assert.NotEqual(first, second);
        Assert.Equal(second, (await FindCourseAsync(courseId)).ThumbnailKey);
        Assert.DoesNotContain(first, _minio.Objects.Keys);
        Assert.Contains(second, _minio.Objects.Keys);
    }

    [Fact]
    public async Task Thumbnail_AdminOnInstructorCourse_UploadsAndSetsKeyAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        owner.Dispose();
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));

        // Act
        var key = await UploadAndSetAsync(admin, courseId, "image/jpeg");

        // Assert
        Assert.Equal(key, (await FindCourseAsync(courseId)).ThumbnailKey);
    }

    [Theory]
    [InlineData("image/gif", 1024)]
    [InlineData("application/pdf", 1024)]
    public async Task RequestUpload_DisallowedContentType_ReturnsFieldErrorAsync(string contentType, long contentLength)
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var response = await owner.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", new { contentType, contentLength });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("contentType", out _));
    }

    [Theory]
    [InlineData(2 * 1024 * 1024 + 1)]
    [InlineData(0)]
    public async Task RequestUpload_ContentLengthOutOfRange_ReturnsFieldErrorAsync(long contentLength)
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;

        // Act
        var response = await owner.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", new { contentType = "image/png", contentLength });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("contentLength", out _));
    }

    [Fact]
    public async Task Put_FileLargerThanAnnouncedSize_IsRejectedByStorageAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var upload = await RequestUploadAsync(owner, courseId, "image/png", 1024);

        // Act
        var put = await PutFileAsync(upload, "image/png", 5 * 1024 * 1024);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
        Assert.Empty(_minio.Objects);
    }

    [Fact]
    public async Task Put_FileOfOtherTypeThanAnnounced_IsRejectedByStorageAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var upload = await RequestUploadAsync(owner, courseId, "image/png", 1024);

        // Act
        var put = await PutFileAsync(upload, "text/html", 1024);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
        Assert.Empty(_minio.Objects);
    }

    [Fact]
    public async Task SetThumbnail_FileNeverUploaded_ReturnsFieldErrorAndKeepsCourseAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var upload = await RequestUploadAsync(owner, courseId, "image/png", 1024);

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail", new { objectKey = upload.ObjectKey });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("objectKey", out _));
        Assert.Null((await FindCourseAsync(courseId)).ThumbnailKey);
    }

    [Fact]
    public async Task SetThumbnail_KeyOfOtherCourse_ReturnsFieldErrorAndKeepsCourseAsync()
    {
        // Arrange
        var (client, courseId) = await CreateOwnerWithCourseAsync();
        using var owner = client;
        var foreignKey = $"{Guid.NewGuid():N}/{Guid.NewGuid():N}.png";
        _minio.Objects[foreignKey] = [];

        // Act
        var response = await owner.PutAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail", new { objectKey = foreignKey });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null((await FindCourseAsync(courseId)).ThumbnailKey);
    }

    [Fact]
    public async Task Thumbnail_InstructorOfOtherCourse_ReturnsForbiddenAndKeepsCourseAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        var upload = await RequestUploadAsync(owner, courseId, "image/png", 1024);
        await PutFileAsync(upload, "image/png", 1024);
        owner.Dispose();
        using var attacker = await CreateAuthorizedClientAsync((await CreateInstructorAsync()).UserId);

        // Act
        var requestUpload = await attacker.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", new { contentType = "image/png", contentLength = 10 });
        var set = await attacker.PutAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail", new { objectKey = upload.ObjectKey });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, requestUpload.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, set.StatusCode);
        Assert.Null((await FindCourseAsync(courseId)).ThumbnailKey);
    }

    [Fact]
    public async Task Thumbnail_StudentAndNoToken_AreRejectedAsync()
    {
        // Arrange
        var (owner, courseId) = await CreateOwnerWithCourseAsync();
        owner.Dispose();
        using var student = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Student));
        using var anonymous = Host.CreateClient();
        var body = new { contentType = "image/png", contentLength = 10 };

        // Act
        var asStudent = await student.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", body);
        var asGuest = await anonymous.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", body);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, asStudent.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, asGuest.StatusCode);
    }

    [Fact]
    public async Task Thumbnail_CourseNotFound_ReturnsNotFoundAsync()
    {
        // Arrange
        using var admin = await CreateAuthorizedClientAsync(await Host.CreateUserAsync(RoleNames.Admin));
        var courseId = Guid.NewGuid();

        // Act
        var response = await admin.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", new { contentType = "image/png", contentLength = 10 });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> UploadAndSetAsync(HttpClient client, Guid courseId, string contentType)
    {
        var upload = await RequestUploadAsync(client, courseId, contentType, 2048);
        (await PutFileAsync(upload, contentType, 2048)).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail", new { objectKey = upload.ObjectKey })).EnsureSuccessStatusCode();
        return upload.ObjectKey;
    }

    private static async Task<UploadTicket> RequestUploadAsync(HttpClient client, Guid courseId, string contentType, long contentLength)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/courses/{courseId}/thumbnail-upload", new { contentType, contentLength });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new UploadTicket(new Uri(body.GetProperty("uploadUrl").GetString()!), body.GetProperty("objectKey").GetString()!);
    }

    /// <summary>Tải file thẳng lên MinIO giả bằng url đã cấp, không đi qua API (đúng luồng trình duyệt).</summary>
    private async Task<HttpResponseMessage> PutFileAsync(UploadTicket upload, string contentType, int length)
    {
        using var storage = new HttpClient(_minio, disposeHandler: false);
        using var content = new ByteArrayContent(new byte[length]);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return await storage.PutAsync(upload.Url, content);
    }

    private async Task<CourseAggregate> FindCourseAsync(Guid courseId)
    {
        await using var scope = Host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return dbContext.Set<CourseAggregate>().Single(course => course.CourseId == courseId);
    }

    private async Task<(HttpClient Client, Guid CourseId)> CreateOwnerWithCourseAsync()
    {
        var instructor = await CreateInstructorAsync();
        var client = await CreateAuthorizedClientAsync(instructor.UserId);
        var categoryId = await CreateCategoryAsync();
        var response = await client.PostAsJsonAsync("/api/v1/courses", new { categoryId, title = "Khóa mẫu", price = 100000 });
        response.EnsureSuccessStatusCode();
        return (client, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("courseId").GetGuid());
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

    private async Task<HttpClient> CreateAuthorizedClientAsync(Guid userId)
    {
        var tokens = await Host.LoginAsync(userId);
        var client = Host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("access_token").GetString());
        return client;
    }

    private sealed record UploadTicket(Uri Url, string ObjectKey);
}
