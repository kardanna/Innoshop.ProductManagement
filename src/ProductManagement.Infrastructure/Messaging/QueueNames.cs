using System.Collections.ObjectModel;
using Innoshop.Contracts.UserManagement;

namespace ProductManagement.Infrastructure.Messaging;

public static class QueueNames
{
    private const string Prefix = "ProductManagement";
    public static readonly ReadOnlyDictionary<string, string> Names = 
        new( 
            new Dictionary<string, string>
            {
                [$"{UserDeactivatedMessage.RoutingKey}"] = $"{Prefix}.{UserDeactivatedMessage.RoutingKey}",
                [$"{UserReactivatedMessage.RoutingKey}"] = $"{Prefix}.{UserReactivatedMessage.RoutingKey}",
                [$"{UserDeletedMessage.RoutingKey}"] = $"{Prefix}.{UserDeletedMessage.RoutingKey}"
            }
        );
}