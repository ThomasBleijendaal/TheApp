namespace TheApp.BackgroundServices.ServiceBus.Abstractions;

public interface IMessage
{
    public BinaryData Data { get; }
}

