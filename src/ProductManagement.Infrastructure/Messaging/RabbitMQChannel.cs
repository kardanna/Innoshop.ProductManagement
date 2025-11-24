using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using ProductManagement.Infrastructure.Messaging.Options;
using Innoshop.Contracts.UserManagement;
using System.Collections.ObjectModel;

namespace ProductManagement.Infrastructure.Messaging;

public class RabbitMQChannel : IExchangeChannel
{
    private readonly ILogger<RabbitMQChannel> _logger;
    private readonly RabbitMQOptions _options;
    private  IConnection _connection = null!;
    private  IChannel _channel = null!;
    private readonly TaskCompletionSource _tcs = new();

    public IChannel Channel => _channel;
    public IReadOnlyDictionary<string, string> DeclaredQueueNames { get; private set; } = null!;
    public Task InitializationComplete => _tcs.Task;
    
    public RabbitMQChannel(
        ILogger<RabbitMQChannel> logger,
        IOptions<RabbitMQOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task Initialize()
    {
        if (_channel is not null) return;

        var factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _logger.LogInformation($"Established connection to RabbitMQ");

        _channel = await _connection.CreateChannelAsync();
        _logger.LogInformation($"Created a channel");

        var exchangeName = Innoshop.Contracts.UserManagement.Exchange.Name;
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic
        );
        _logger.LogInformation("Declared '{ExchangeName}' exchange", exchangeName);

        var dict = new Dictionary<string, string>();

        var userDeactivatedMessageQueue = await _channel.QueueDeclareAsync
        (
            queue: $"{Service.Name}.{UserDeactivatedMessage.RoutingKey}",
            durable: true,
            exclusive: true,
            autoDelete: false,
            passive: false,
            noWait: false
        );

        await _channel.QueueBindAsync(
            queue: userDeactivatedMessageQueue.QueueName,
            exchange: Innoshop.Contracts.UserManagement.Exchange.Name,
            routingKey: UserDeactivatedMessage.RoutingKey
        );

        dict.Add(UserDeactivatedMessage.RoutingKey, userDeactivatedMessageQueue.QueueName);

        var userReactivatedMessageQueue = await _channel.QueueDeclareAsync
        (
            queue: $"{Service.Name}.{UserReactivatedMessage.RoutingKey}",
            durable: true,
            exclusive: true,
            autoDelete: false,
            passive: false,
            noWait: false
        );

        await _channel.QueueBindAsync(
            queue: userReactivatedMessageQueue.QueueName,
            exchange: Innoshop.Contracts.UserManagement.Exchange.Name,
            routingKey: UserReactivatedMessage.RoutingKey
        );

        dict.Add(UserReactivatedMessage.RoutingKey, userReactivatedMessageQueue.QueueName);

        var userDeletedMessageQueue = await _channel.QueueDeclareAsync
        (
            queue: $"{Service.Name}.{UserDeletedMessage.RoutingKey}",
            durable: true,
            exclusive: true,
            autoDelete: false,
            passive: false,
            noWait: false
        );   

        await _channel.QueueBindAsync(
            queue: userDeletedMessageQueue.QueueName,
            exchange: Innoshop.Contracts.UserManagement.Exchange.Name,
            routingKey: UserDeletedMessage.RoutingKey
        );

        dict.Add(UserDeletedMessage.RoutingKey, userDeletedMessageQueue.QueueName);

        DeclaredQueueNames = new ReadOnlyDictionary<string, string>(dict);

        _tcs.SetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Channel != null) await Channel.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ channel");

        if (_connection != null) await _connection.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ connection");
    }
}