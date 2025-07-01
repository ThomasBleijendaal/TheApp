using System.Collections.Concurrent;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheApp.DistributedConcurrency;
using TheApp.DistributedConcurrency.Internals;

namespace TheApp.DistributedConcurrency.Blob;

internal class BlobLeaseSemaphoreStorage : ISemaphoreStorage, IAsyncDisposable
{
    private readonly IHostApplicationLifetime _hostApplication;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobLeaseSemaphoreStorage> _logger;

    private readonly ConcurrentDictionary<string, string> _leases = new();

    public BlobLeaseSemaphoreStorage(
        IHostApplicationLifetime hostApplication,
        BlobServiceClient blobServiceClient,
        ILogger<BlobLeaseSemaphoreStorage> logger)
    {
        _hostApplication = hostApplication;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<bool> TryTakeSemaphoreAsync(string ticket)
    {
        var blobLeaseClient = await GetBlobLeaseClientAsync(ticket);

        try
        {
            var lease = await blobLeaseClient.AcquireAsync(TimeSpan.FromSeconds(60));
            _leases[ticket] = lease.Value.LeaseId;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ReleaseSemaphoreAsync(string ticket)
    {
        if (_leases.TryGetValue(ticket, out var leaseId))
        {
            try
            {
                var client = await GetBlobLeaseClientAsync(ticket, leaseId);
                await client.ReleaseAsync();

                _leases.TryRemove(ticket, out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release semaphore");
            }
        }
        else
        {
            _logger.LogWarning("Tried to release unknown semaphore");
        }
    }

    public async Task RenewLeasesAsync(CancellationToken token)
    {
        foreach (var (ticket, leaseId) in _leases.ToArray())
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var client = await GetBlobLeaseClientAsync(ticket, leaseId);
                await client.RenewAsync(cancellationToken: token);

                _logger.LogInformation("Renewed lease for semaphore {ticket}", ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew lease for {ticket}", ticket);
                _leases.TryRemove(ticket, out _);

                if (await TryTakeSemaphoreAsync(ticket))
                {
                    _logger.LogWarning(ex, "Re-acquired lease for {ticket}", ticket);
                }
                else
                {
                    _logger.LogError(ex, "Failed to re-acquire lease for {ticket} -- stopping application", ticket);
                    _hostApplication.StopApplication();
                }
            }
        }
    }

    private async Task<BlobLeaseClient> GetBlobLeaseClientAsync(string ticket, string? leaseId = null)
    {
        var blobContainer = _blobServiceClient.GetBlobContainerClient("semaphores");
        await blobContainer.CreateIfNotExistsAsync();

        var blobClient = blobContainer.GetBlockBlobClient(ticket);
        if (!await blobClient.ExistsAsync())
        {
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(ticket)));
        }

        var blobLeaseClient = blobClient.GetBlobLeaseClient(leaseId);
        return blobLeaseClient;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (ticket, _) in _leases)
        {
            try
            {
                await ReleaseSemaphoreAsync(ticket);
            }
            catch { }
        }
    }
}
