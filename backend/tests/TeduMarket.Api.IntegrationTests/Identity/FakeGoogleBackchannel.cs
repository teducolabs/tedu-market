using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>
/// Giả HTTP tới Google: endpoint token đổi <c>code</c> thành access token cùng giá trị, endpoint userinfo trả hồ sơ đã đăng ký cho code đó.
/// </summary>
internal sealed class FakeGoogleBackchannel : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _profiles = new();

    /// <summary>Đăng ký hồ sơ Google sẽ trả khi client dùng <paramref name="code"/>.</summary>
    public void SetProfile(string code, string subject, string email, bool emailVerified = true) =>
        _profiles[code] = new Dictionary<string, object>
        {
            ["sub"] = subject,
            ["email"] = email,
            ["email_verified"] = emailVerified,
            ["name"] = "Học viên thử",
        };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Post)
        {
            var form = await request.Content!.ReadAsStringAsync(cancellationToken);
            var code = System.Web.HttpUtility.ParseQueryString(form)["code"];
            return Json(new { access_token = code, token_type = "Bearer", expires_in = 3600 });
        }

        var token = request.Headers.Authorization?.Parameter;
        return token is not null && _profiles.TryGetValue(token, out var profile)
            ? Json(profile)
            : new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }

    private static HttpResponseMessage Json(object body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
}
