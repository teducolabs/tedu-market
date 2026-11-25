using System.Runtime.CompilerServices;

namespace TeduMarket.Api.IntegrationTests.Common;

/// <summary>
/// Tắt seeding Admin lúc khởi động cho mọi host kiểm thử (nhiều host không có DB hoặc chưa migrate);
/// test seeding gọi <c>AdminSeeder</c> và <c>AdminSeedHostedService</c> trực tiếp.
/// </summary>
internal static class TestEnvironment
{
#pragma warning disable CA2255 // Cố ý đặt biến môi trường một lần trước mọi test của assembly
    [ModuleInitializer]
    internal static void DisableAdminSeed() => Environment.SetEnvironmentVariable("AdminSeed__Enabled", "false");
#pragma warning restore CA2255
}
