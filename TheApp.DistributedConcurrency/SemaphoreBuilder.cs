using Microsoft.Extensions.DependencyInjection;
using TheApp.DistributedConcurrency.Abstractions;
using TheApp.DistributedConcurrency.Internals;
using TheApp.DistributedConcurrency.Semaphores;

namespace TheApp.DistributedConcurrency;

public static class SemaphoreBuilder
{
    public static IDistributedConcurrencySemaphore Build(IServiceProvider sp, SemaphoreType type, string ticket)
    {
        if (type == SemaphoreType.Unbounded)
        {
            return new UnboundedSemaphore();
        }
        else if (type == SemaphoreType.Singleton)
        {
            return new SingletonConcurrencySemaphore(sp.GetRequiredService<SemaphoreService>(), ticket);
        }
        else
        {
            throw new NotSupportedException("Unsupported semaphore type");
        }
    }
}
