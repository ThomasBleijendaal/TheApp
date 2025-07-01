namespace TheApp.BackgroundServices.Timer.Abstractions;

public interface ITimerProcessor
{
    Task RunAsync(CancellationToken token);
}
