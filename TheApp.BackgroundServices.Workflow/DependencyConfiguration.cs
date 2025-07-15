using DurableMediator.HostedService.Extensions;
using DurableTask.AzureStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheApp.BackgroundServices.Timer;

public static class DependencyConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAzureStorageWorkflows()
        {
            services.AddAzureStorageOrchestrationService(false);

            services.AddWorkflowService(sp => sp.GetRequiredService<AzureStorageOrchestrationService>());
            services.AddDurableMediator(sp => sp.GetRequiredService<AzureStorageOrchestrationService>());

            return services;
        }
    }

    // .NET 10 preview has issues with this method in the new format
    private static IServiceCollection AddAzureStorageOrchestrationService(this IServiceCollection services, bool useAppLease)
    {
        services.AddTransient(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var storageConnectionString = config.GetConnectionString("BlobStorage")
               ?? throw new InvalidOperationException();

            var taskHubName = config["TaskHubName"] ?? throw new InvalidOperationException();

            var azureStorageSettings = new AzureStorageOrchestrationServiceSettings
            {
                StorageAccountClientProvider = new StorageAccountClientProvider(storageConnectionString),
                TaskHubName = taskHubName,
                UseAppLease = useAppLease,
                AppName = Guid.NewGuid().ToString(),
                AppLeaseOptions = new()
                {
                    AcquireInterval = TimeSpan.FromSeconds(5),
                    LeaseInterval = TimeSpan.FromSeconds(15),
                    RenewInterval = TimeSpan.FromSeconds(5)
                },
                MaxConcurrentTaskActivityWorkItems = 2,
                WorkerId = Guid.NewGuid().ToString()
            };

            return new AzureStorageOrchestrationService(azureStorageSettings);
        });

        return services;
    }
}
