using RabbitMQ.Client;

namespace ProductManagement.Infrastructure.Messaging.Abstractions;

public interface IRabbitMQConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken);
}