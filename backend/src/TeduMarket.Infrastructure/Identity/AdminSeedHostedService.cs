using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>Chạy <see cref="AdminSeeder"/> một lần lúc API khởi động (migration đã áp bằng migrator trước đó).</summary>
public sealed class AdminSeedHostedService(IServiceScopeFactory scopeFactory, IOptions<AdminSeedOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
