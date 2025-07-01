using Microsoft.Extensions.Hosting;

namespace TheApp.DistributedConcurrency.Internals;

internal class SemaphoreRenewalBackgroundService : BackgroundService
{
    private readonly ISemaphoreStorage _storage;

    public SemaphoreRenewalBackgroundService(
        ISemaphoreStorage storage)
    {
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await _storage.RenewLeasesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}
