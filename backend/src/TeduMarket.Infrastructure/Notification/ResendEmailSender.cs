using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TeduMarket.Application.Notification;

namespace TeduMarket.Infrastructure.Notification;

/// <summary>
/// Gửi email qua REST API của Resend (<c>POST /emails</c>); không dùng SDK để khỏi thêm thư viện
/// ([log](../../../../docs/decision-logs/2026-11-26-gui-email-resend-qua-http-o-t18.md)).
/// Mọi lỗi được bọc thành <see cref="EmailDeliveryException"/> không chứa khóa API.
/// </summary>
public sealed class ResendEmailSender(IHttpClientFactory httpClientFactory, IOptions<ResendOptions> options) : IEmailSender
{
    public const string HttpClientName = "resend";

    private const int _errorBodyMaxLength = 500;

    public async Task SendAsync(EmailMessage message, string idempotencyKey, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new EmailDeliveryException("Thiếu cấu hình Resend:ApiKey hoặc Resend:FromAddress");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = settings.FromAddress,
                to = new[] { message.To },
                subject = message.Subject,
                html = message.HtmlBody,
                text = message.TextBody,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var excerpt = body.Length > _errorBodyMaxLength ? body[.._errorBodyMaxLength] : body;
                throw new EmailDeliveryException($"Resend trả {(int)response.StatusCode}: {excerpt}");
            }
        }
        catch (HttpRequestException exception)
        {
            throw new EmailDeliveryException("Không gọi được Resend", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new EmailDeliveryException("Resend không phản hồi kịp", exception);
        }
    }
}
