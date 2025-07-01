using Microsoft.Extensions.DependencyInjection;
using TheApp.DistributedConcurrency.Internals;

namespace TheApp.DistributedConcurrency;

public static class DependencyConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDistributedConcurrencyServices()
        {
            services.AddSingleton<SemaphoreService>();

            services.AddHostedService<DistributedConcurrencyBackgroundService>();
            services.AddHostedService<SemaphoreRenewalBackgroundService>();

            return services;
        }
    }
}
