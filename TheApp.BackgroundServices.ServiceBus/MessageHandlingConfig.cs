using TheApp.DistributedConcurrency;

namespace TheApp.BackgroundServices.ServiceBus;

public record MessageHandlingConfig
{
    public required string QueueName { get; init; }

    public int BoundedCapacity { get; set; } = 100;

    public int MaxDegreeOfParallelism { get; set; } = 10;

    public SemaphoreType ConcurrencyType { get; set; } = SemaphoreType.Unbounded;
}
