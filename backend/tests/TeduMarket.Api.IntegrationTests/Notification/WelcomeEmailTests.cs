using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TeduMarket.Api.IntegrationTests.Common;
using TeduMarket.Api.IntegrationTests.Identity;
using TeduMarket.Application.Notification;
using TeduMarket.Domain.Outbox;
using TeduMarket.Infrastructure.Outbox;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Api.IntegrationTests.Notification;

/// <summary>Luồng: Học viên đăng nhập Google lần đầu nhận Email chào mừng — T18, FR-27, NFR-18.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresTestGroup.Name)]
public sealed class WelcomeEmailTests(PostgresFixture postgres) : IAsyncLifetime, IDisposable
{
    private const string _email = "hoc.vien@example.test";
    private const string _subject = "google-sub-1";

    private readonly FakeGoogleBackchannel _google = new();
    private readonly FakeResendBackchannel _resend = new();
    private readonly FakeTimeProvider _clock = new(DateTimeOffset.UtcNow);
    private IdentityTestHost? _host;

    private IdentityTestHost Host => _host!;

    public async Task InitializeAsync() => await StartHostAsync(withResend: true);

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    public void Dispose() => _host?.Dispose();

    [Fact]
    public async Task ProcessBatch_GoogleFirstSignIn_SendsExactlyOneWelcomeEmailWithCourseListLinkAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        await GoogleLoginTests.SignInWithGoogleAsync(client, "code-1");
        var pending = await GetWelcomeMessagesAsync();

        // Act
        await ProcessAsync();

        // Assert
        var message = Assert.Single(pending);
        var sent = Assert.Single(_resend.Received);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal("/emails", sent.Path);
        Assert.Equal($"Bearer {IdentityTestHost.ResendApiKey}", sent.Authorization);
        Assert.Equal(message.OutboxMessageId.ToString(), sent.IdempotencyKey);
        Assert.Equal(_email, sent.Body.GetProperty("to")[0].GetString());
        Assert.Contains("https://hoc.example.test/courses", sent.Body.GetProperty("html").GetString(), StringComparison.Ordinal);
        Assert.Equal(OutboxMessageStatus.Processed, (await GetWelcomeMessagesAsync()).Single().Status);
    }

    [Fact]
    public async Task ProcessBatch_GoogleSecondSignIn_SendsNoSecondWelcomeEmailAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email);
        _google.SetProfile("code-2", _subject, _email);
        using var firstClient = Host.CreateHttpsClient();
        await GoogleLoginTests.SignInWithGoogleAsync(firstClient, "code-1");
        await ProcessAsync();
        using var secondClient = Host.CreateHttpsClient();
        await GoogleLoginTests.SignInWithGoogleAsync(secondClient, "code-2");

        // Act
        await ProcessAsync();

        // Assert
        Assert.Single(await GetWelcomeMessagesAsync());
        Assert.Single(_resend.Received);
    }

    [Fact]
    public async Task ProcessBatch_GoogleSignInRejected_WritesNoWelcomeMessageAsync()
    {
        // Arrange
        _google.SetProfile("code-1", _subject, _email, emailVerified: false);
        using var client = Host.CreateHttpsClient();
        await GoogleLoginTests.SignInWithGoogleAsync(client, "code-1");

        // Act
        await ProcessAsync();

        // Assert
        Assert.Empty(await GetWelcomeMessagesAsync());
        Assert.Empty(_resend.Received);
    }

    [Fact]
    public async Task ProcessBatch_ResendFails_SignInStillSucceedsAndMessageIsRetriedLaterAsync()
    {
        // Arrange
        _resend.StatusCode = HttpStatusCode.InternalServerError;
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        var final = await GoogleLoginTests.SignInWithGoogleAsync(client, "code-1");
        await ProcessAsync();
        var afterFailure = (await GetWelcomeMessagesAsync()).Single();
        _resend.StatusCode = HttpStatusCode.OK;
        _clock.Advance(OutboxMessage.BaseRetryDelay + TimeSpan.FromSeconds(1));

        // Act
        await ProcessAsync();

        // Assert
        Assert.Contains("code=", final.Headers.Location!.Query, StringComparison.Ordinal);
        Assert.Equal(OutboxMessageStatus.Pending, afterFailure.Status);
        Assert.Equal(1, afterFailure.AttemptCount);
        Assert.Contains("500", afterFailure.LastError, StringComparison.Ordinal);
        var retried = (await GetWelcomeMessagesAsync()).Single();
        Assert.Equal(OutboxMessageStatus.Processed, retried.Status);
        Assert.Equal(2, _resend.Received.Count);
        Assert.Equal(_resend.Received.First().IdempotencyKey, _resend.Received.Last().IdempotencyKey);
    }

    [Fact]
    public async Task ProcessBatch_ResendNotConfigured_KeepsMessagePendingWithoutCallingResendAsync()
    {
        // Arrange
        await DisposeAsync();
        await StartHostAsync(withResend: false);
        _google.SetProfile("code-1", _subject, _email);
        using var client = Host.CreateHttpsClient();
        await GoogleLoginTests.SignInWithGoogleAsync(client, "code-1");

        // Act
        await ProcessAsync();

        // Assert
        var message = (await GetWelcomeMessagesAsync()).Single();
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Contains("Resend:ApiKey", message.LastError, StringComparison.Ordinal);
        Assert.Empty(_resend.Received);
    }

    private async Task StartHostAsync(bool withResend)
    {
        _host = new IdentityTestHost(await postgres.CreateDatabaseAsync(), _google, withResend ? _resend : null, _clock);
        await _host.RegisterSampleClientAsync();
    }

    private Task<int> ProcessAsync() => Host.Services.GetRequiredService<OutboxProcessor>().ProcessBatchAsync(CancellationToken.None);

    private async Task<List<OutboxMessage>> GetWelcomeMessagesAsync()
    {
        await using var scope = Host.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Set<OutboxMessage>()
            .AsNoTracking()
            .Where(message => message.Type == WelcomeEmailPayload.MessageType)
            .ToListAsync();
    }
}
