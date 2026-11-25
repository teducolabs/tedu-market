using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TeduMarket.Api.IntegrationTests.Identity;

/// <summary>Cấu hình khóa OpenIddict ngoài Development (T15 sửa lỗi khóa ký đối xứng của T13).</summary>
[Trait("Category", "Integration")]
public sealed class SigningKeyConfigurationTests
{
    [Fact]
    public async Task Request_ProductionWithRsaSigningKeyAsPem_StartsAndServesAsync()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        await using var factory = CreateFactory(Convert.ToBase64String(new byte[32]), rsa.ExportPkcs8PrivateKeyPem());
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("bm90LWEtcnNhLWtleQ==")]
    [InlineData("!!!không-phải-base64!!!")]
    public void CreateClient_SigningKeyNotRsa_FailsFastAtStartup(string signingKey)
    {
        // Arrange
        using var factory = CreateFactory(Convert.ToBase64String(new byte[32]), signingKey);

        // Act
        var act = () => factory.CreateClient();

        // Assert
        var exception = Assert.ThrowsAny<Exception>(act);
        Assert.Contains("OpenIddict:SigningKey", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CreateClient_ProductionWithoutKeys_FailsFastAtStartup()
    {
        // Arrange
        using var factory = CreateFactory(string.Empty, string.Empty);

        // Act
        var act = () => factory.CreateClient();

        // Assert
        var exception = Assert.ThrowsAny<Exception>(act);
        Assert.Contains("OpenIddict:EncryptionKey", exception.ToString(), StringComparison.Ordinal);
    }

    private static WebApplicationFactory<Program> CreateFactory(string encryptionKey, string signingKey) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.UseSetting("OpenIddict:EncryptionKey", encryptionKey);
            builder.UseSetting("OpenIddict:SigningKey", signingKey);
        });
}
