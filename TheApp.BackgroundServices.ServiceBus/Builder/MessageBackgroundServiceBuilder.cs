using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheApp.BackgroundServices.ServiceBus.Abstractions;
using TheApp.BackgroundServices.ServiceBus.Internals;
using TheApp.DistributedConcurrency;
using TheApp.ServiceResolver;

namespace TheApp.BackgroundServices.ServiceBus.Builder;

internal class MessageBackgroundServiceBuilder<TProcessor> : IDistributedServiceRegistration
    where TProcessor : class, IMessageProcessor
{
    private readonly MessageHandlingConfig _config;

    public MessageBackgroundServiceBuilder(
        MessageHandlingConfig config,
        IServiceCollection services)
    {
        _config = config;
    }

    public MessageBackgroundService Build(IServiceProvider sp)
    {
        var semaphoreTicket = TicketBuilder.Build(_config.QueueName, typeof(TProcessor), typeof(MessageBackgroundService));

        var logger = sp.GetRequiredService<ILogger<MessageBackgroundService>>();

        var resolver = new ScopedServiceResolver<MessageHandler>(sp, sp =>
        {
            var processor = (IMessageProcessor)sp.GetRequiredService<TProcessor>();
            var logger = sp.GetRequiredService<ILogger<MessageHandler>>();

            // TODO: MessageSource could be parameterized
            var source = ActivatorUtilities.CreateInstance<MessageSource>(sp, _config);
            var pipeline = ActivatorUtilities.CreateInstance<MessagePipeline>(sp, processor, _config);
            var buffer = ActivatorUtilities.CreateInstance<MessageBuffer>(sp, pipeline);

            return new(source, pipeline, buffer, _config, logger);
        });

        var semaphore = SemaphoreBuilder.Build(sp, _config.ConcurrencyType, semaphoreTicket);

        return new MessageBackgroundService(semaphore, _config, resolver, logger);
    }

    public IDistributedServiceRegistration AsGlobalSingleton()
    {
        _config.ConcurrencyType = SemaphoreType.GlobalSingleton;
        return this;
    }

    public IDistributedServiceRegistration AsSingletonPerInstance()
    {
        _config.ConcurrencyType = SemaphoreType.SingletonPerInstance;
        return this;
    }
}
