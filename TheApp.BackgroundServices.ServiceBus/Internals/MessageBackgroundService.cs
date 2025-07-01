using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheApp.DistributedConcurrency.Abstractions;
using TheApp.ServiceResolver;

namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal class MessageBackgroundService : BackgroundService
{
    private readonly IDistributedConcurrencySemaphore _semaphore;
    private readonly MessageHandlingConfig _config;
    private readonly IScopedServiceResolver<MessageHandler> _serviceResolver;
    private readonly ILogger<MessageBackgroundService> _logger;

    public MessageBackgroundService(
        IDistributedConcurrencySemaphore semaphore,
        MessageHandlingConfig config,
        IScopedServiceResolver<MessageHandler> serviceResolver,
        ILogger<MessageBackgroundService> logger)
    {
        _semaphore = semaphore;
        _config = config;
        _serviceResolver = serviceResolver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting queue handler for queue {queue}", _config.QueueName);

        await using var semaphore = await _semaphore.AcquireAsync(stoppingToken);

        _logger.LogInformation("Acquired lock for handling {queue}", _config.QueueName);

        do
        {
            try
            {
                await using var scope = _serviceResolver.ResolveService();

                await scope.Service.HandleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during queue");
            }
        }
        while (!stoppingToken.IsCancellationRequested);

        _logger.LogInformation("Completed queue handler for queue {queue}", _config.QueueName);
    }
}
