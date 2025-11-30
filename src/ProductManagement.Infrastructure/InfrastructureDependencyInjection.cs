using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Infrastructure.Messaging;
using ProductManagement.Infrastructure.Messaging.Abstractions;

namespace ProductManagement.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static void AddProductManagementInfrastructure(this IServiceCollection services)
    {
        //Messaging
        services.AddSingleton<RabbitMQConnectionProvider>();
        services.AddSingleton<IRabbitMQConnectionProvider>(sp => sp.GetRequiredService<RabbitMQConnectionProvider>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConnectionProvider>());

        services.AddSingleton<RabbitMQConfigurator>();
        services.AddSingleton<IRabbitMQConfigurator>(sp => sp.GetRequiredService<RabbitMQConfigurator>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConfigurator>());

        services.AddHostedService<RabbitMQConsumer>();
    }
}