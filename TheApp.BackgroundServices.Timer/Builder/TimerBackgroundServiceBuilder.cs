using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheApp.BackgroundServices.ServiceBus.Abstractions;
using TheApp.BackgroundServices.Timer;
using TheApp.BackgroundServices.Timer.Abstractions;
using TheApp.BackgroundServices.Timer.Internals;
using TheApp.DistributedConcurrency;
using TheApp.ServiceResolver;

namespace TheApp.BackgroundServices.Timer.Builder;

internal class TimerBackgroundServiceBuilder<TProcessor> : IDistributedServiceRegistration
    where TProcessor : class, ITimerProcessor
{
    private readonly TimerConfig _config;

    public TimerBackgroundServiceBuilder(
        TimerConfig config,
        IServiceCollection services)
    {
        _config = config;
    }

    public TimerBackgroundService<TProcessor> Build(IServiceProvider sp)
    {
        var semaphoreTicket = TicketBuilder.Build(_config.CronExpression, typeof(TProcessor), typeof(TimerBackgroundService<TProcessor>));

        var logger = sp.GetRequiredService<ILogger<TimerBackgroundService<TProcessor>>>();
        var semaphore = SemaphoreBuilder.Build(sp, _config.ConcurrencyType, semaphoreTicket);

        var resolver = new ScopedServiceResolver<TProcessor>(sp, sp => sp.GetRequiredService<TProcessor>());

        return new TimerBackgroundService<TProcessor>(semaphore, _config, resolver, logger);
    }

    public IDistributedServiceRegistration AsSingleton()
    {
        _config.ConcurrencyType = SemaphoreType.Singleton;
        return this;
    }

    public IDistributedServiceRegistration Unbounded()
    {
        _config.ConcurrencyType = SemaphoreType.Unbounded;
        return this;
    }
}
