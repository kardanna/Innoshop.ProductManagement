using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using RabbitMQ.Client;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQConfigurator : IHostedService
{
    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly ILogger<RabbitMQConfigurator> _logger;

    public RabbitMQConfigurator(
        IRabbitMQConnectionProvider connectionProvider,
        ILogger<RabbitMQConfigurator> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuring RabbitMQ exchange and queues...");
        
        _logger.LogInformation("Fetching RabbitMQ connection...");
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _logger.LogInformation("Successfully fetched RabbitMQ connection.");

        _logger.LogInformation("Creating RabbitMQ connection...");
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully created RabbitMQ connection.");

        var exchangeName = Innoshop.Contracts.UserManagement.Exchange.Name;
       
        _logger.LogInformation("Declaring '{ExchangeName}' exchange...", exchangeName);
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            cancellationToken: cancellationToken
        );
        _logger.LogInformation("Successfully declared '{ExchangeName}' exchange.", exchangeName);
        
        await DeclareAndBindQueueAsync(channel, UserDeactivatedMessage.RoutingKey, exchangeName, cancellationToken);

        await DeclareAndBindQueueAsync(channel, UserReactivatedMessage.RoutingKey, exchangeName, cancellationToken);

        await DeclareAndBindQueueAsync(channel, UserDeletedMessage.RoutingKey, exchangeName, cancellationToken);

        _logger.LogInformation("Successfully configured RabbitMQ exchange and queues...");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task DeclareAndBindQueueAsync(
        IChannel channel,
        string topicName,
        string exchangeName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Declaring queue for '{TopicName}' topic...", topicName);
        
        var queue = await channel.QueueDeclareAsync
        (
            queue: QueueNames.Names[topicName],
            durable: true,
            exclusive: true,
            autoDelete: false,
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Successfully declared '{QueueName}' queue for '{TopicName}' topic.",
            queue.QueueName, topicName);

        _logger.LogInformation("Binding '{QueueName}' queue to '{ExchangeName}' exchange...",
            queue.QueueName, exchangeName);

        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: topicName,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Successfully binded '{QueueName}' queue to '{ExchangeName}' exchange.",
            queue.QueueName, exchangeName);
    }
}