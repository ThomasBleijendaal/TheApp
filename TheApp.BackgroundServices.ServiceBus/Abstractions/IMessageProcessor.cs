using TheApp.BackgroundServices.ServiceBus;

namespace TheApp.BackgroundServices.ServiceBus.Abstractions;

public interface IMessageProcessor
{
    Task<MessageResult> ProcessMessageAsync(IMessage message);
}
