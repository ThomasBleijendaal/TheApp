using Microsoft.Extensions.DependencyInjection;

namespace TheApp.ServiceResolver;

public class ScopedServiceResolver<TService> : IScopedServiceResolver<TService>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TService> _builder;

    public ScopedServiceResolver(IServiceProvider serviceProvider, Func<IServiceProvider, TService> builder)
    {
        _serviceProvider = serviceProvider;
        _builder = builder;
    }

    public Scope<TService> ResolveService()
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var service = _builder.Invoke(scope.ServiceProvider);

        return new(service, scope);
    }
}
