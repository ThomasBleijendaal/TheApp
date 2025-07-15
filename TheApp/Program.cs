using DurableMediator.HostedService.Extensions;
using TheApp.BackgroundServices.ServiceBus;
using TheApp.BackgroundServices.Timer;
using TheApp.DistributedConcurrency;
using TheApp.DistributedConcurrency.Blob;
using TheApp.ServiceBus;
using TheApp.Timer;
using TheApp.Workflow;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddLogging(logging => logging.AddConsole());

builder.Services.AddOpenApi();

builder.Services.AddBlobLeaseStorage(config);
builder.Services.AddDistributedConcurrencyServices();

builder.Services.AddServiceBusClient(config);
builder.Services.AddQueueHandler<QueueMessageProcessor>("queue").AsSingleton();

builder.Services.AddTimer<TimerProcessor>("@every_second").AsSingleton();

builder.Services.AddAzureStorageWorkflows();
builder.Services.AddWorkflow<LogWorkflow>();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
