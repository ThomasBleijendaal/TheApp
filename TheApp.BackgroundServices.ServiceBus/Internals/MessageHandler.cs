using Microsoft.Extensions.Logging;

namespace TheApp.BackgroundServices.ServiceBus.Internals;

internal class MessageHandler
{
    private readonly IMessageSource _messageSource;
    private readonly MessagePipeline _messagePipeline;
    private readonly MessageBuffer _messageBuffer;
    private readonly MessageHandlingConfig _config;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(
        IMessageSource messageSource,
        MessagePipeline messagePipeline,
        MessageBuffer messageBuffer,
        MessageHandlingConfig config,
        ILogger<MessageHandler> logger)
    {
        _messageSource = messageSource;
        _messagePipeline = messagePipeline;
        _messageBuffer = messageBuffer;
        _config = config;
        _logger = logger;
    }

    public async Task HandleAsync(CancellationToken stoppingToken)
    {
        try
        {
            var readTask = ReadMessagesAsync(stoppingToken);
            var bufferTask = ProcessBufferAsync(stoppingToken);

            await Task.WhenAll(readTask, bufferTask);
        }
        catch
        {
            _logger.LogInformation("Completing queue handler for queue {queue}", _config.QueueName);

            await _messagePipeline.CompleteAsync();
        }

        _logger.LogInformation("Completed queue handler for queue {queue}", _config.QueueName);
    }

    private async Task ReadMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var messageBatch in _messageSource.GetMessages(cancellationToken))
        {
            _logger.LogInformation("Buffering messages from {queue}", _config.QueueName);

            await _messageBuffer.BufferMessagesAsync(messageBatch);
        }
    }

    private async Task ProcessBufferAsync(CancellationToken cancellationToken)
    {
        await _messageBuffer.ProcessBufferAsync(cancellationToken);
    }
}

