using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheApp.BackgroundServices.Timer.Abstractions;
using TheApp.DistributedConcurrency.Abstractions;
using TheApp.ServiceResolver;

namespace TheApp.BackgroundServices.Timer.Internals;

internal class TimerBackgroundService<TProcessor> : BackgroundService
    where TProcessor : class, ITimerProcessor
{
    private readonly IDistributedConcurrencySemaphore _semaphore;
    private readonly TimerConfig _config;
    private readonly IScopedServiceResolver<TProcessor> _serviceResolver;
    private readonly ILogger<TimerBackgroundService<TProcessor>> _logger;

    public TimerBackgroundService(
        IDistributedConcurrencySemaphore semaphore,
        TimerConfig config,
        IScopedServiceResolver<TProcessor> serviceResolver,
        ILogger<TimerBackgroundService<TProcessor>> logger)
    {
        _semaphore = semaphore;
        _config = config;
        _serviceResolver = serviceResolver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting timer handler for timer {cron}", _config.CronExpression);

        var expression = CronExpression.Parse(_config.CronExpression);

        await using var semaphore = await _semaphore.AcquireAsync(stoppingToken);

        _logger.LogInformation("Acquired lock for handling {cron}", _config.CronExpression);

        do
        {
            try
            {
                var nextOccurrence = expression.GetNextOccurrence(DateTime.UtcNow);
                if (nextOccurrence == null)
                {
                    _logger.LogInformation("Stopping timer handler for timer {cron}", _config.CronExpression);
                    break;
                }

                var offset = nextOccurrence.Value - DateTime.UtcNow;
                if (offset.TotalSeconds > 130)
                {
                    // wait for 2 minutes and check later
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                    continue;
                }

                await using var scope = _serviceResolver.ResolveService();

                offset = nextOccurrence.Value - DateTime.UtcNow;
                if (offset.TotalSeconds > 0)
                {
                    await Task.Delay(offset, stoppingToken);
                }

                // TODO: in some cases this gets invoked too often
                await scope.Service.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during timer");
            }
        }
        while (!stoppingToken.IsCancellationRequested);

        _logger.LogInformation("Stopped timer handler for timer {cron}", _config.CronExpression);
    }
}
