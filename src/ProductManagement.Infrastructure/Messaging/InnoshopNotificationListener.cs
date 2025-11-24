using System.Text;
using System.Text.Json;
using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductManagement.Infrastructure.Messaging;

public class InnoshopNotificationListener : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IExchangeChannel _exchangeChannel;
    private readonly ILogger<InnoshopNotificationListener> _logger;

    public InnoshopNotificationListener(
        IServiceScopeFactory scopeFactory,
        IExchangeChannel channel,
        ILogger<InnoshopNotificationListener> logger)
    {
        _scopeFactory = scopeFactory;
        _exchangeChannel = channel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _exchangeChannel.InitializationComplete;
        //if (_exchangeChannel.Channel is null) throw new UninitializedExchangeChannelException();
        
        var userDeactivatedMessageConsumer = new AsyncEventingBasicConsumer(_exchangeChannel.Channel);
        userDeactivatedMessageConsumer.ReceivedAsync += async (model, ea) =>
        {
            _logger.LogInformation("Recieved user deactivation message");
            
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            UserDeactivatedMessage? message;
            
            try
            {
                message = JsonSerializer.Deserialize<UserDeactivatedMessage>(messageString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse message body to an appropriate object");

                await ((AsyncEventingBasicConsumer)model).Channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false //increase the counter and requeue??????
                );
                
                return;
            }

            if (message is null) return;
            
            using var scope = _scopeFactory.CreateScope();
            var productOwnerService = scope.ServiceProvider.GetRequiredService<IProductOwnerService>();
            
            var result = await productOwnerService.DeactivateProductOwnerAsync(message.UserId);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to deactivate user '{OwnerId}'. Error code: '{ErrorCode}'. Error description: '{ErrorDescription}'",
                    message.UserId, result.Error.Code, result.Error.Description
                );

                await ((AsyncEventingBasicConsumer)model).Channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false
                );

                return;
            }

            _logger.LogInformation("Successfully makred user '{OwnerId}' as deactivated", message.UserId);

            await ((AsyncEventingBasicConsumer)model).Channel.BasicAckAsync(
                deliveryTag: ea.DeliveryTag,
                multiple: false
            );
        };

        if (!_exchangeChannel.DeclaredQueueNames.TryGetValue(UserDeactivatedMessage.RoutingKey, out var userDeactivatedMessageQueueName))
        {
            _logger.LogCritical("Cannot find registered queue for recieving messages with '{RoutingKey}' routing key. Exiting...", UserDeactivatedMessage.RoutingKey);
            Environment.FailFast($"Cannot find queue for recieving messages with '{UserDeactivatedMessage.RoutingKey}' routing key. Exiting...");
        }

        await _exchangeChannel.Channel.BasicConsumeAsync(
            queue: userDeactivatedMessageQueueName,
            autoAck: false,
            consumer: userDeactivatedMessageConsumer
        );

        if (!_exchangeChannel.DeclaredQueueNames.TryGetValue(UserReactivatedMessage.RoutingKey, out var userReactivatedMessageQueueName))
        {
            _logger.LogCritical("Cannot find registered queue for recieving messages with '{RoutingKey}' routing key. Exiting...", UserReactivatedMessage.RoutingKey);
            Environment.FailFast($"Cannot find queue for recieving messages with '{UserReactivatedMessage.RoutingKey}' routing key. Exiting...");
        }

        /*await _exchangeChannel.Channel.BasicConsumeAsync(
            queue: userReactivatedMessageQueueName,
            autoAck: false,
            consumer: userReactivatedMessageConsumer
        );*/

        if (!_exchangeChannel.DeclaredQueueNames.TryGetValue(UserReactivatedMessage.RoutingKey, out var userDeletedQueueName))
        {
            _logger.LogCritical("Cannot find registered queue for recieving messages with '{RoutingKey}' routing key. Exiting...", UserReactivatedMessage.RoutingKey);
            Environment.FailFast($"Cannot find queue for recieving messages with '{UserReactivatedMessage.RoutingKey}' routing key. Exiting...");
        }

        /*await _exchangeChannel.Channel.BasicConsumeAsync(
            queue: userDeletedQueueName,
            autoAck: false,
            consumer: userDeletedMessageConsumer
        );*/
    }
}