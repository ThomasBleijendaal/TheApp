namespace TheApp.BackgroundServices.ServiceBus.Abstractions;

public interface IDistributedServiceRegistration
{
    IDistributedServiceRegistration AsGlobalSingleton();

    IDistributedServiceRegistration AsSingletonPerInstance();
}
