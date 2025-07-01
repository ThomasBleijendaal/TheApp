using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheApp.DistributedConcurrency.Internals;

internal class DistributedConcurrencyBackgroundService : BackgroundService
{
    private readonly SemaphoreService _semaphoreService;
    private readonly ISemaphoreStorage _semaphoreStorage;
    private readonly ILogger<DistributedConcurrencyBackgroundService> _logger;

    public DistributedConcurrencyBackgroundService(
        SemaphoreService semaphoreService,
        ISemaphoreStorage semaphoreStorage,
        ILogger<DistributedConcurrencyBackgroundService> logger)
    {
        _semaphoreService = semaphoreService;
        _semaphoreStorage = semaphoreStorage;
        _logger = logger;
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
