namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal class MessageBuffer
{
    private readonly List<ReceivedMessage> _messages = new();
    private readonly MessagePipeline _messagePipeline;

    public MessageBuffer(
        MessagePipeline messagePipeline)
    {
        _messagePipeline = messagePipeline;
    }

    public async Task BufferMessagesAsync(IEnumerable<ReceivedMessage> messageBatch)
    {
        _messages.AddRange(messageBatch);

        foreach (var message in messageBatch)
        {
            await _messagePipeline.SendAsync(message);
        }
    }

    public async Task ProcessBufferAsync(CancellationToken token)
    {
        do
        {
            _messages.RemoveAll(x => x.ProcessingResult.HasValue);

            // TODO: async parallelize
            foreach (var message in _messages)
            {
                try
                {
                    await message.RenewLocksAsync();
                }
                catch (Exception ex)
                {
                    // TODO: check for expired lock and remove message
                    await message.DeadLetterAsync(ex.Message);
                }
            }

            await Task.Delay(1000, token);
        }
        while (true);
    }
}
