using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TeduMarket.Api.IntegrationTests.Notification;

/// <summary>Giả HTTP tới Resend (<c>POST /emails</c>): ghi lại request và trả mã trạng thái do test đặt.</summary>
internal sealed class FakeResendBackchannel : HttpMessageHandler
{
    public ConcurrentQueue<ReceivedEmail> Received { get; } = new();

    /// <summary>Mã trả cho các request sau; đặt 5xx để giả lập Resend lỗi.</summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        Received.Enqueue(new ReceivedEmail(
            request.Method,
            request.RequestUri!.AbsolutePath,
            request.Headers.Authorization?.ToString(),
            request.Headers.TryGetValues("Idempotency-Key", out var keys) ? keys.Single() : null,
            JsonDocument.Parse(body).RootElement.Clone(),
            StatusCode));
        return new HttpResponseMessage(StatusCode)
        {
            Content = new StringContent("""{"id":"email-1"}""", Encoding.UTF8, "application/json"),
        };
    }

    internal sealed record ReceivedEmail(HttpMethod Method, string Path, string? Authorization, string? IdempotencyKey, JsonElement Body, HttpStatusCode RespondedWith);
}
