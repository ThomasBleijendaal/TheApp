
namespace TheApp.ServiceResolver;

public record Scope<TService> : IAsyncDisposable
{
    private readonly IAsyncDisposable _disposable;

    internal Scope(TService service, IAsyncDisposable disposable)
    {
        Service = service;
        _disposable = disposable;
    }

    public TService Service { get; }

    public ValueTask DisposeAsync() => _disposable.DisposeAsync();
}
