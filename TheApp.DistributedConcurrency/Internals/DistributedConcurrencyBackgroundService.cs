using Microsoft.Extensions.Hosting;

namespace TheApp.DistributedConcurrency.Internals;

internal class DistributedConcurrencyBackgroundService : BackgroundService
{
    private readonly SemaphoreService _semaphoreService;

    public DistributedConcurrencyBackgroundService(SemaphoreService semaphoreService)
    {
        _semaphoreService = semaphoreService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await _semaphoreService.ProcessPendingSemaphoresAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}
