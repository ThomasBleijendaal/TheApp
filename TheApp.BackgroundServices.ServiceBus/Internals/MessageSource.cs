using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;
using TheApp.BackgroundServices.ServiceBus;

namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal class MessageSource : IMessageSource
{
    private readonly ServiceBusClient _client;
    private readonly MessageHandlingConfig _config;

    public MessageSource(ServiceBusClient serviceBusClient, MessageHandlingConfig config)
    {
        _client = serviceBusClient;
        _config = config;
    }

    public async IAsyncEnumerable<IEnumerable<ReceivedMessage>> GetMessages([EnumeratorCancellation] CancellationToken token)
    {
        var receiver = _client.CreateReceiver(_config.QueueName);

        do
        {
            var messages = await receiver.ReceiveMessagesAsync(100, TimeSpan.FromMinutes(1), token);

            yield return messages.Select(message => new ReceivedMessage(message, receiver));
        }
        while (!token.IsCancellationRequested);
    }
}
