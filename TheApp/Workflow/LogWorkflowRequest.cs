using DurableMediator.HostedService;

namespace TheApp.Workflow;

public record LogWorkflowRequest(string Message) : IWorkflowRequest
{
    public string WorkflowName => nameof(LogWorkflow);

    public string? InstanceId => Guid.NewGuid().ToString();
}
