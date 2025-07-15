using DurableMediator.HostedService;

namespace TheApp.Workflow;

public class LogWorkflow : IWorkflow<LogWorkflowRequest>
{
    public async Task OrchestrateAsync(IWorkflowExecution<LogWorkflowRequest> execution)
    {
        execution.ReplaySafeLogger.LogInformation("Starting workflow");

        await execution.SendAsync(new LogActionRequest($"{execution.Request.Message}-1"));
        await execution.SendAsync(new LogActionRequest($"{execution.Request.Message}-2"));
        await execution.SendAsync(new LogActionRequest($"{execution.Request.Message}-3"));
        await execution.SendAsync(new LogActionRequest($"{execution.Request.Message}-4"));

        execution.ReplaySafeLogger.LogInformation("Finished workflow");
    }
}
