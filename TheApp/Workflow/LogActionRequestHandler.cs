using MediatR;

namespace TheApp.Workflow;

public class LogActionRequestHandler : IRequestHandler<LogActionRequest>
{
    private readonly ILogger<LogActionRequestHandler> _logger;

    public LogActionRequestHandler(
        ILogger<LogActionRequestHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LogActionRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.Message);

        await Task.Delay(400, cancellationToken);
    }
}
