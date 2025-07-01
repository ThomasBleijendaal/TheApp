using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TheApp.DistributedConcurrency.Internals;

internal class SemaphoreService
{
    private readonly ISemaphoreStorage _semaphoreStorage;
    private readonly ILogger<SemaphoreService> _logger;

    private readonly ConcurrentDictionary<string, SemaphoreState> _semaphores = new();
    private TaskCompletionSource _requestCts = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public SemaphoreService(
        ISemaphoreStorage semaphoreStorage,
        ILogger<SemaphoreService> logger)
    {
        _semaphoreStorage = semaphoreStorage;
        _logger = logger;
    }

    public Task<IAsyncDisposable> GetSemaphoreAsync(string ticket, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<IAsyncDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);
        token.Register(() => tcs.TrySetCanceled());

        _semaphores[ticket] = new(ticket, tcs);

        var currentTcs = _requestCts;
        _requestCts = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        currentTcs.SetResult();

        return tcs.Task;
    }

    public async Task ReleaseSemaphoreAsync(string ticket)
    {
        await _semaphoreStorage.ReleaseSemaphoreAsync(ticket);
        _semaphores.TryRemove(ticket, out _);
    }

    public async Task ProcessPendingSemaphoresAsync(CancellationToken token)
    {
        var task = _requestCts.Task;

        // nothing to do, return the semaphore requested task that completes when there is something to do
        if (_semaphores.All(x => x.Value.Tcs.Task.IsCompleted))
        {
            await task.WaitAsync(token);
        }
        else
        {
            await Task.WhenAll(_semaphores
                .Where(x => !x.Value.Tcs.Task.IsCompleted)
                .Select(x => x.Value)
                .Select(TryAcquireSemaphoreAsync));
        }
    }

    private async Task TryAcquireSemaphoreAsync(SemaphoreState state)
    {
        if (state.RetryAfter > DateTimeOffset.UtcNow)
        {
            return;
        }

        if (await _semaphoreStorage.TryTakeSemaphoreAsync(state.Ticket))
        {
            state.Tcs.SetResult(new Semaphore(this, state.Ticket, _logger));
        }
        else
        {
            _logger.LogInformation("Could not acquire semaphore for {ticket}", state.Ticket);

            state.RetryCount++;

            state.RetryAfter = DateTimeOffset.UtcNow.AddSeconds(Math.Min(30, state.RetryCount));
        }
    }

    private sealed record SemaphoreState(string Ticket, TaskCompletionSource<IAsyncDisposable> Tcs)
    {
        public uint RetryCount { get; set; }

        public DateTimeOffset RetryAfter { get; set; } = DateTimeOffset.UtcNow;
    }
}
