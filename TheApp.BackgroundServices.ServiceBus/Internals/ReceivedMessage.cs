using Azure.Messaging.ServiceBus;
using TheApp.BackgroundServices.ServiceBus.Abstractions;

namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal record ReceivedMessage : IMessage
{
    private readonly ServiceBusReceiver _receiver;

    public ReceivedMessage(
        ServiceBusReceivedMessage message,
        ServiceBusReceiver receiver)
    {
        Message = message;
        _receiver = receiver;
    }

    public ServiceBusReceivedMessage Message { get; init; }

    public BinaryData Data => Message.Body;

    public Result? ProcessingResult { get; private set; }

    public async Task RenewLocksAsync()
    {
        if (ProcessingResult == null)
        {
            try
            {
                await _receiver.RenewMessageLockAsync(Message);
            }
            catch { }
        }
    }

    public async Task CompleteAsync()
    {
        ProcessingResult = Result.Complete;
        await _receiver.CompleteMessageAsync(Message);
    }

    public async Task DeadLetterAsync(string reason)
    {
        ProcessingResult = Result.DeadLetter;
        await _receiver.DeadLetterMessageAsync(Message, reason);
    }

    // TODO: retry
}

