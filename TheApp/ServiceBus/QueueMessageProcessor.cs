using TheApp.BackgroundServices.ServiceBus;
using TheApp.BackgroundServices.ServiceBus.Abstractions;

namespace TheApp.ServiceBus;

public class QueueMessageProcessor : IMessageProcessor
{
    public async Task<MessageResult> ProcessMessageAsync(IMessage message)
    {
        await Task.Delay(100);

        if (Random.Shared.Next(10) == 0)
        {
            return new MessageResult(Result.DeadLetter, null);
        }
        else
        {
            return new(Result.Complete, null);
        }
    }
}
