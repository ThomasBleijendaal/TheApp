using DurableMediator.HostedService;
using TheApp.BackgroundServices.Timer.Abstractions;
using TheApp.Workflow;

namespace TheApp.Timer;

public class TimerProcessor : ITimerProcessor
{
    private readonly ILogger<TimerProcessor> _logger;
    private readonly IWorkflowService _workflowService;

    public TimerProcessor(
        ILogger<TimerProcessor> logger,
        IWorkflowService workflowService)
    {
        _logger = logger;
        _workflowService = workflowService;
    }

    public async Task RunAsync(CancellationToken token)
    {
        _logger.LogInformation("Timer invoked");

        await _workflowService.StartWorkflowAsync(new LogWorkflowRequest(DateTimeOffset.UtcNow.ToString()));
    }
}
