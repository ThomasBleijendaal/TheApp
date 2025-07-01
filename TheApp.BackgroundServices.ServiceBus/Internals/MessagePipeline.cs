using System.Threading.Tasks.Dataflow;
using TheApp.BackgroundServices.ServiceBus.Abstractions;

namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal class MessagePipeline
{
    private readonly IMessageProcessor _processor;
    private readonly MessageHandlingConfig _config;

    private readonly ActionBlock<ReceivedMessage> _target;

    public MessagePipeline(
        IMessageProcessor processor,
        MessageHandlingConfig config)
    {
        _processor = processor;
        _config = config;
        _target = new ActionBlock<ReceivedMessage>(HandleMessageAsync, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = config.BoundedCapacity,
            EnsureOrdered = true,
            MaxDegreeOfParallelism = config.MaxDegreeOfParallelism
        });
    }

    public async Task SendAsync(ReceivedMessage message)
    {
        await _target.SendAsync(message);
    }

    public async Task CompleteAsync()
    {
        _target.Complete();
        await _target.Completion;
    }

    private async Task HandleMessageAsync(ReceivedMessage message)
    {
        try
        {
            var result = await _processor.ProcessMessageAsync(message);

            switch (result.Result)
            {
                case Result.Complete:
                    await message.CompleteAsync();
                    break;

                case Result.Retry:
                    // TODO: implement sender
                    await message.CompleteAsync();
                    break;

                case Result.DeadLetter:
                default:
                    await message.DeadLetterAsync(result.Exception?.Message ?? "Unknown failure");
                    break;
            }
        }
        catch (Exception ex)
        {
            await message.DeadLetterAsync(ex.Message);
        }
    }
}
