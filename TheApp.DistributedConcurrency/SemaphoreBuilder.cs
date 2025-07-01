using Microsoft.Extensions.DependencyInjection;
using TheApp.DistributedConcurrency.Abstractions;
using TheApp.DistributedConcurrency.Internals;
using TheApp.DistributedConcurrency.Semaphores;

namespace TheApp.DistributedConcurrency;

public static class SemaphoreBuilder
{
    public static IDistributedConcurrencySemaphore Build(IServiceProvider sp, SemaphoreType type, string ticket)
    {
        if (type == SemaphoreType.SingletonPerInstance)
        {
            return new SingletonPerInstanceConcurrencySemaphore();
        }
        else if (type == SemaphoreType.GlobalSingleton)
        {
            return new GlobalSingletonConcurrencySemaphore(sp.GetRequiredService<SemaphoreService>(), ticket);
        }
        else
        {
            throw new NotSupportedException("Unsupported semaphore type");
        }
    }
}
