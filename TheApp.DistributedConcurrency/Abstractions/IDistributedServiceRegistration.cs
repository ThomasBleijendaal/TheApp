namespace TheApp.BackgroundServices.ServiceBus.Abstractions;

public interface IDistributedServiceRegistration
{
    IDistributedServiceRegistration AsSingleton();

    IDistributedServiceRegistration Unbounded();
}
