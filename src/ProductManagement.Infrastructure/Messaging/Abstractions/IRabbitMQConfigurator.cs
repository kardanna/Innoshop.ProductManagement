using System.Collections.ObjectModel;

namespace ProductManagement.Infrastructure.Messaging.Abstractions;

public interface IRabbitMQConfigurator
{
    Task Configure(CancellationToken cancellationToken);
    ReadOnlyDictionary<string, string> DeclaredQueueNames { get; }
}