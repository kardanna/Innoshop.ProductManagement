using RabbitMQ.Client;

namespace ProductManagement.Infrastructure.Messaging.Abstractions;

public interface IExchangeChannel : IAsyncDisposable
{
    public IChannel Channel { get; }
    IReadOnlyDictionary<string, string> DeclaredQueueNames { get; }
    public Task InitializationComplete { get; }
    Task Initialize();
}