using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheApp.BackgroundServices.ServiceBus;
using TheApp.BackgroundServices.ServiceBus.Abstractions;
using TheApp.BackgroundServices.ServiceBus.Builder;

namespace TheApp.BackgroundServices.ServiceBus;

public static class DependencyConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddServiceBusClient(IConfiguration config)
            => services.AddSingleton(new ServiceBusClient(config.GetConnectionString("ServiceBus")));

        public IDistributedServiceRegistration AddQueueHandler<TProcessor>(string queueName, Action<MessageHandlingConfig>? setup = null)
            where TProcessor : class, IMessageProcessor
        {
            var config = new MessageHandlingConfig
            {
                QueueName = queueName
            };
            setup?.Invoke(config);

            var builder = new MessageBackgroundServiceBuilder<TProcessor>(config, services);

            // TODO: this should not be singleton
            services.AddSingleton<TProcessor>();

            services.AddHostedService(builder.Build);

            return builder;
        }
    }
}
