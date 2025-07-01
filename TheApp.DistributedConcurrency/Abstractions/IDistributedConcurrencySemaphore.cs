namespace TheApp.DistributedConcurrency.Abstractions;

public interface IDistributedConcurrencySemaphore
{
    Task<IAsyncDisposable> AcquireAsync(CancellationToken token);
}
