using TheApp.BackgroundServices.Timer.Abstractions;

namespace TheApp.Timer;

public class TimerProcessor : ITimerProcessor
{
    private readonly ILogger<TimerProcessor> _logger;

    public TimerProcessor(
        ILogger<TimerProcessor> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(CancellationToken token)
    {
        _logger.LogInformation("Timer invoked");

        return Task.CompletedTask;
    }
}
