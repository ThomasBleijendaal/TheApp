namespace TheApp.ServiceResolver;

public interface IScopedServiceResolver<TService>
{
    Scope<TService> ResolveService();
}
