using TheApp.DistributedConcurrency.Abstractions;
using TheApp.DistributedConcurrency.Internals;

namespace TheApp.DistributedConcurrency.Semaphores;

internal class GlobalSingletonConcurrencySemaphore : IDistributedConcurrencySemaphore
{
    private readonly SemaphoreService _semaphoreService;
    private readonly string _ticket;

    public GlobalSingletonConcurrencySemaphore(
        SemaphoreService semaphoreService,
        string ticket)
    {
        _semaphoreService = semaphoreService;
        _ticket = ticket;
    }

    public Task<IAsyncDisposable> AcquireAsync(CancellationToken token)
        => _semaphoreService.GetSemaphoreAsync(_ticket, token);
}
