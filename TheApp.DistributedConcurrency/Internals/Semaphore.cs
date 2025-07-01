using Microsoft.Extensions.Logging;

namespace TheApp.DistributedConcurrency.Internals;

internal class Semaphore : IAsyncDisposable
{
    private readonly SemaphoreService _semaphoreService;
    private readonly string _ticket;
    private readonly ILogger _logger;

    public Semaphore(SemaphoreService semaphoreService, string ticket, ILogger logger)
    {
        _semaphoreService = semaphoreService;
        _ticket = ticket;
        _logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Releasing semaphore {ticket}", _ticket);
        await _semaphoreService.ReleaseSemaphoreAsync(_ticket);
    }
}
