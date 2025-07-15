using MediatR;

namespace TheApp.Workflow;

public record LogActionRequest(string Message) : IRequest;
