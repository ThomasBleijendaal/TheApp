namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal interface IMessageSource
{
    IAsyncEnumerable<IEnumerable<ReceivedMessage>> GetMessages(CancellationToken token);
}
