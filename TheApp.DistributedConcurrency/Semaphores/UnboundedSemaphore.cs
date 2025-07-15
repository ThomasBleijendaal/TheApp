using TheApp.DistributedConcurrency.Abstractions;

namespace TheApp.DistributedConcurrency.Semaphores;

internal class UnboundedSemaphore : IDistributedConcurrencySemaphore
{
    public Task<IAsyncDisposable> AcquireAsync(CancellationToken token)
        => Task.FromResult<IAsyncDisposable>(new CompletedDisposable());

    private sealed class CompletedDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
