namespace TheApp.BackgroundServices.ServiceBus;

public enum Result
{
    Retry = 1,
    DeadLetter = 2,
    Complete = 3
}

