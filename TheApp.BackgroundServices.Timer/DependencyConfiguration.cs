using Microsoft.Extensions.DependencyInjection;
using TheApp.BackgroundServices.ServiceBus.Abstractions;
using TheApp.BackgroundServices.Timer.Abstractions;
using TheApp.BackgroundServices.Timer.Builder;

namespace TheApp.BackgroundServices.Timer;

public static class DependencyConfiguration
{
    extension(IServiceCollection services)
    {
        public IDistributedServiceRegistration AddTimer<TProcessor>(string cronExpression)
            where TProcessor : class, ITimerProcessor
        {
            var config = new TimerConfig { CronExpression = cronExpression };

            var builder = new TimerBackgroundServiceBuilder<TProcessor>(config, services);

            services.AddScoped<TProcessor>();

            services.AddHostedService(builder.Build);

            return builder;
        }
    }
}
