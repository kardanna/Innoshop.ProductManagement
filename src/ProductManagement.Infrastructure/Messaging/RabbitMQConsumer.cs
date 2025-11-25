using System.Text;
using System.Text.Json;
using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Interfaces;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQConsumer : IHostedService
{
    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumer> _logger;

    private IChannel _userDeactivatedChannel = null!;
    private string? _userDeactivatedConsumerTag;
    private IChannel _userReactivatedChannel = null!;
    private string? _userReactivatedConsumerTag;
    private IChannel _userDeletedChannel = null!;
    private string? _userDeletedConsumerTag;

    public RabbitMQConsumer(
        IRabbitMQConnectionProvider connectionProvider,
        IServiceScopeFactory scopefactory,
        ILogger<RabbitMQConsumer> logger)
    {
        _connectionProvider = connectionProvider;
        _scopeFactory = scopefactory;
        _logger = logger;
    }

    //REFACTOR!!!!
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching RabbitMQ connection...");
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _logger.LogInformation("Successfully fetched RabbitMQ connection.");

        _logger.LogInformation("Creating a channel for consuming {TopicName} messages...", UserDeactivatedMessage.RoutingKey);
        _userDeactivatedChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _userDeactivatedChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully created a channel for consuming {TopicName} messages...", UserDeactivatedMessage.RoutingKey);

        var userDeactivatedMessageQueueName = QueueNames.Names[UserDeactivatedMessage.RoutingKey];
        _logger.LogInformation("Binding {MessageObject} message handler to {QueueName} queue...", nameof(UserDeactivatedMessage), userDeactivatedMessageQueueName);
        var userDeactivatedMessageConsumer = new AsyncEventingBasicConsumer(_userDeactivatedChannel);
        userDeactivatedMessageConsumer.ReceivedAsync += UserDeactivatedMessageHandler;
        _userDeactivatedConsumerTag = await _userDeactivatedChannel.BasicConsumeAsync(
            queue: userDeactivatedMessageQueueName,
            autoAck: false,
            consumer: userDeactivatedMessageConsumer,
            cancellationToken: cancellationToken
        );
        _logger.LogInformation("Successfully binded {MessageObject} message handler to {QueueName} queue...", nameof(UserDeactivatedMessage), userDeactivatedMessageQueueName);

        _logger.LogInformation("Creating a channel for consuming {TopicName} messages...", UserReactivatedMessage.RoutingKey);
        _userReactivatedChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _userReactivatedChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully created a channel for consuming {TopicName} messages...", UserReactivatedMessage.RoutingKey);

        var userReactivatedMessageQueueName = QueueNames.Names[UserReactivatedMessage.RoutingKey];
        _logger.LogInformation("Binding {MessageObject} message handler to {QueueName} queue...", nameof(UserReactivatedMessage), userReactivatedMessageQueueName);
        var userReactivatedMessageConsumer = new AsyncEventingBasicConsumer(_userReactivatedChannel);
        userReactivatedMessageConsumer.ReceivedAsync += UserReactivatedMessageHandler;
        _userReactivatedConsumerTag = await _userReactivatedChannel.BasicConsumeAsync(
            queue: userReactivatedMessageQueueName,
            autoAck: false,
            consumer: userReactivatedMessageConsumer,
            cancellationToken: cancellationToken
        );
        _logger.LogInformation("Successfully binded {MessageObject} message handler to {QueueName} queue...", nameof(UserReactivatedMessage), userReactivatedMessageQueueName);

        _logger.LogInformation("Creating a channel for consuming {TopicName} messages...", UserDeletedMessage.RoutingKey);
        _userDeletedChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _userDeletedChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully created a channel for consuming {TopicName} messages...", UserDeletedMessage.RoutingKey);

        var userDeletedMessageQueueName = QueueNames.Names[UserDeletedMessage.RoutingKey];
        _logger.LogInformation("Binding {MessageObject} message handler to {QueueName} queue...", nameof(UserDeletedMessage), userDeletedMessageQueueName);
        var userDeletedMessageConsumer = new AsyncEventingBasicConsumer(_userDeletedChannel);
        userDeletedMessageConsumer.ReceivedAsync += UserDeletedMessageHandler;
        _userDeletedConsumerTag = await _userDeletedChannel.BasicConsumeAsync(
            queue: userDeletedMessageQueueName,
            autoAck: false,
            consumer: userDeletedMessageConsumer,
            cancellationToken: cancellationToken
        );
        _logger.LogInformation("Successfully binded {MessageObject} message handler to {QueueName} queue...", nameof(UserDeletedMessage), userDeletedMessageQueueName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing the channel for consuming {TopicName} messages...", UserDeactivatedMessage.RoutingKey);
        if (_userDeactivatedConsumerTag is not null) await _userDeactivatedChannel.BasicCancelAsync(_userDeactivatedConsumerTag, cancellationToken: cancellationToken);
        if (_userDeactivatedChannel != null) await _userDeactivatedChannel.CloseAsync(cancellationToken);
        _logger.LogInformation("Successfully closed the channel for consuming {TopicName} messages.", UserDeactivatedMessage.RoutingKey);

        _logger.LogInformation("Closing the channel for consuming {TopicName} messages...", UserReactivatedMessage.RoutingKey);
        if (_userReactivatedConsumerTag is not null) await _userReactivatedChannel.BasicCancelAsync(_userReactivatedConsumerTag, cancellationToken: cancellationToken);
        if (_userReactivatedChannel != null) await _userReactivatedChannel.CloseAsync(cancellationToken);
        _logger.LogInformation("Successfully closed the channel for consuming {TopicName} messages.", UserReactivatedMessage.RoutingKey);

        _logger.LogInformation("Closing the channel for consuming {TopicName} messages...", UserDeletedMessage.RoutingKey);
        if (_userDeletedConsumerTag is not null) await _userDeletedChannel.BasicCancelAsync(_userDeletedConsumerTag, cancellationToken: cancellationToken);
        if (_userDeletedChannel != null) await _userDeletedChannel.CloseAsync(cancellationToken);
        _logger.LogInformation("Successfully closed the channel for consuming {TopicName} messages.", UserDeletedMessage.RoutingKey);
    }

    private async Task UserDeactivatedMessageHandler(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            _logger.LogInformation("Recieved user deactivation message '{MessageId}'. Processing...", ea.BasicProperties.MessageId);
        
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserDeactivatedMessage>(messageString);
            
            using var scope = _scopeFactory.CreateScope();
            var productOwnerService = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
            var result = await productOwnerService.DeactivateProductOwnerAsync(message!.UserId);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to deactivate user '{OwnerId}'. Error code: '{ErrorCode}'. Error description: '{ErrorDescription}'",
                    message.UserId,
                    result.Error.Code,
                    result.Error.Description);

                await _userDeactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
                return;
            }

            _logger.LogInformation("Successfully makred user '{OwnerId}' as deactivated", message.UserId);

            await _userDeactivatedChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);            
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse '{MessageId}' message body to '{MessageObject}' object.",
                ea.BasicProperties.MessageId,
                nameof(UserDeactivatedMessage));
            await _userDeactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proccess user deactivation message '{MessageId}'.", ea.BasicProperties.MessageId);
            await _userDeactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
    }

    private async Task UserReactivatedMessageHandler(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            _logger.LogInformation("Recieved user reactivation message '{MessageId}'. Processing...", ea.BasicProperties.MessageId);
        
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserReactivatedMessage>(messageString);
            
            using var scope = _scopeFactory.CreateScope();
            var productOwnerService = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
            var result = await productOwnerService.ReactivateProductOwnerAsync(message!.UserId);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to reactivate user '{OwnerId}'. Error code: '{ErrorCode}'. Error description: '{ErrorDescription}'",
                    message.UserId,
                    result.Error.Code,
                    result.Error.Description);

                await _userReactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
                return;
            }

            _logger.LogInformation("Successfully makred user '{OwnerId}' as reactivated", message.UserId);

            await _userReactivatedChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);            
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse '{MessageId}' message body to '{MessageObject}' object.",
                ea.BasicProperties.MessageId,
                nameof(UserReactivatedMessage));
            await _userReactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proccess user reactivation message '{MessageId}'.", ea.BasicProperties.MessageId);
            await _userReactivatedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
    }

    private async Task UserDeletedMessageHandler(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            _logger.LogInformation("Recieved user deleted message '{MessageId}'. Processing...", ea.BasicProperties.MessageId);
        
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<UserDeletedMessage>(messageString);
            
            using var scope = _scopeFactory.CreateScope();
            var productOwnerService = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
            var result = await productOwnerService.DeleteProductOwnerAsync(message!.UserId);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to delete user '{OwnerId}'. Error code: '{ErrorCode}'. Error description: '{ErrorDescription}'",
                    message.UserId,
                    result.Error.Code,
                    result.Error.Description);

                await _userDeletedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
                return;
            }

            _logger.LogInformation("Successfully makred user '{OwnerId}' as deleted", message.UserId);

            await _userDeletedChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);            
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse '{MessageId}' message body to '{MessageObject}' object.",
                ea.BasicProperties.MessageId,
                nameof(UserDeletedMessage));
            await _userDeletedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proccess deactivation message '{MessageId}'.", ea.BasicProperties.MessageId);
            await _userDeletedChannel.BasicNackAsync(ea.DeliveryTag, false, false); //TODO: implement retry
        }
    }
}