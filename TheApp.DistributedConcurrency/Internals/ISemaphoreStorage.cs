namespace TheApp.DistributedConcurrency.Internals;

public interface ISemaphoreStorage
{
    Task<bool> TryTakeSemaphoreAsync(string ticket);

    Task RenewLeasesAsync(CancellationToken token);

    Task ReleaseSemaphoreAsync(string ticket);
}
