using System.Text;
using System.Text.Json;
using Innoshop.Contracts.UserManagement.UserEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Shared;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQConsumer : IHostedService
{
    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly IRabbitMQConfigurator _configurator;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumer> _logger;

    private readonly Dictionary<string, Func<UserEventMessage, Task<Result>>> messageHandlers = new();
    private readonly Dictionary<string, (string ConsumerTag, IChannel Channel)> createdConsumers = new();

    private IConnection? connection;

    public RabbitMQConsumer(
        IRabbitMQConnectionProvider connectionProvider,
        IRabbitMQConfigurator configurator,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMQConsumer> logger)
    {
        _connectionProvider = connectionProvider;
        _configurator = configurator;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    //REFACTOR!!!!
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching RabbitMQ connection...");
        connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _logger.LogInformation("Successfully fetched RabbitMQ connection.");

        AddMessageHandlers();

        await AddConsumerForTopicAsync(UserDeactivatedMessage.Topic, cancellationToken);
        await AddConsumerForTopicAsync(UserReactivatedMessage.Topic, cancellationToken);
        await AddConsumerForTopicAsync(UserDeletedMessage.Topic, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var openedChannel in createdConsumers)
        {
            var topicName = openedChannel.Key;
            var channel = openedChannel.Value.Channel;
            var consumerTag = openedChannel.Value.ConsumerTag;

            _logger.LogInformation("Closing the channel for consuming '{TopicName}' messages...", topicName);

            if (consumerTag is not null) await channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
            if (channel is not null) await channel.CloseAsync(cancellationToken);

            _logger.LogInformation("Successfully closed the channel for consuming '{TopicName}' messages.", topicName);
        }
    }

    private void AddMessageHandlers()
    {
        messageHandlers.Add(UserDeactivatedMessage.Topic, async m =>
        {
           using var scope = _scopeFactory.CreateScope();
           var service = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
           return await service.DeactivateProductOwnerAsync(m.UserId);
        });

        messageHandlers.Add(UserReactivatedMessage.Topic, async m =>
        {
           using var scope = _scopeFactory.CreateScope();
           var service = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
           return await service.ReactivateProductOwnerAsync(m.UserId);  
        });

        messageHandlers.Add(UserDeletedMessage.Topic, async m =>
        {
           using var scope = _scopeFactory.CreateScope();
           var service = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
           return await service.DeleteProductOwnerAsync(m.UserId);  
        });
    }

    private async Task AddConsumerForTopicAsync(string topicName, CancellationToken cancellationToken)
    {
        if (!_configurator.DeclaredQueueNames.TryGetValue(topicName, out var queueName))
        {
            throw new UndeclaredQueueException(topicName);
        }
        await RegisterConsumerAsync(queueName, topicName, cancellationToken);
    }

    private async Task RegisterConsumerAsync(string queueName, string topicName, CancellationToken cancellationToken)
    {
        if (connection is null) throw new UninitializedRabbitMQConnectionException();

        _logger.LogInformation("Creating a channel for consuming '{TopicName}' messages...", topicName);
        
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
        
        _logger.LogInformation("Successfully created a channel for consuming '{TopicName}' messages.", topicName);
        _logger.LogInformation("Creating a consumer for '{TopicName}' messages...", topicName);
        
        var consumer = new AsyncEventingBasicConsumer(channel);        
        consumer.ReceivedAsync += async (sender, ea) => await UserEventsMessageHandler(channel, ea, topicName);

        _logger.LogInformation("Successfully created a consumer for '{TopicName}' messages.", topicName);
        _logger.LogInformation("Binding the consumer to '{QueueName}' queue...", queueName);

        var consumerTag = await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );
        
        _logger.LogInformation("Successfully binded the consumer to '{QueueName}' queue...", queueName);

        createdConsumers.Add(topicName, (consumerTag, channel));
    }

    private async Task UserEventsMessageHandler(IChannel channel, BasicDeliverEventArgs ea, string topicName)
    {
        string messageString = string.Empty;
        try
        {
            _logger.LogInformation("Recieved user event message '{MessageId}'. Processing...", ea.BasicProperties.MessageId);
        
            var body = ea.Body.ToArray();
            messageString = Encoding.UTF8.GetString(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert user deactivation message '{MessageId}' to a string.", ea.BasicProperties.MessageId);
            await channel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
            return;
        }

        UserEventMessage? message = null;
        try
        {
            message = JsonSerializer.Deserialize<UserEventMessage>(messageString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse '{MessageId}' message's body '{MessageBody}' to '{MessageObject}' object.",
                ea.BasicProperties.MessageId,
                messageString,
                nameof(UserEventMessage));
            await channel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
            return;
        }

        try
        {           
            if (!messageHandlers.TryGetValue(topicName, out var handler))
            {
                throw new UndeclaredMessageHandlerException(topicName);
            }
            
            var result = await handler(message!);

            _logger.LogInformation("Processed event message for user '{OwnerId}' with error code '{ErrorCode}'",
                message!.UserId, result.IsSuccess ? "None" : result.Error.Code);

            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proccess user deactivation message '{MessageId}'.", ea.BasicProperties.MessageId);
            await channel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
    }
}