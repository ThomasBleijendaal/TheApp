using TheApp.DistributedConcurrency.Abstractions;
using TheApp.DistributedConcurrency.Internals;

namespace TheApp.DistributedConcurrency.Semaphores;

internal class SingletonConcurrencySemaphore : IDistributedConcurrencySemaphore
{
    private readonly SemaphoreService _semaphoreService;
    private readonly string _ticket;

    public SingletonConcurrencySemaphore(
        SemaphoreService semaphoreService,
        string ticket)
    {
        _semaphoreService = semaphoreService;
        _ticket = ticket;
    }

    public Task<IAsyncDisposable> AcquireAsync(CancellationToken token)
        => _semaphoreService.GetSemaphoreAsync(_ticket, token);
}
