namespace TheApp.BackgroundServices.ServiceBus;

public record struct MessageResult(Result Result, Exception? Exception);

