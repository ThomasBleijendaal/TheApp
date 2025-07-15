using TheApp.DistributedConcurrency;

namespace TheApp.BackgroundServices.Timer;

public record TimerConfig
{
    public required string CronExpression { get; init; }

    public SemaphoreType ConcurrencyType { get; set; } = SemaphoreType.Unbounded;
}
