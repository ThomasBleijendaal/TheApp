using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheApp.DistributedConcurrency;
using TheApp.DistributedConcurrency.Blob;
using TheApp.DistributedConcurrency.Internals;

namespace TheApp.DistributedConcurrency.Blob;

public static class DependencyConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBlobLeaseStorage(IConfiguration config)
        {
            services.AddSingleton(new BlobServiceClient(config.GetConnectionString("BlobStorage")));
            services.AddSingleton<ISemaphoreStorage, BlobLeaseSemaphoreStorage>();

            return services;
        }
    }
}
