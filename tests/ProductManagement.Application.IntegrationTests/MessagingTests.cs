using System.Text;
using System.Text.Json;
using FluentAssertions;
using Innoshop.Contracts.UserManagement.UserEvents;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;


namespace ProductManagement.Application.IntegrationTests;

public class MessagingTests : BaseIntegrationTest
{
    public MessagingTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    private readonly string exchangeName = Exchange.Name;

    [Fact]
    public async Task RabbitMQConsumer_ShouldMarkOwnerAsDeactivated_WhenRecievedMessage()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        
        var ownerId = Guid.CreateVersion7();
        var message = new UserDeactivatedMessage()
        {
            UserId = ownerId,
            TimeStamp = DateTime.UtcNow
        };


        //Act
        await SendMessage(channel, UserDeactivatedMessage.Topic, message);
        var owner = await Eventually(
            func: () => _appContext.ProductOwners.FirstOrDefaultAsync(o => o.Id == ownerId),
            interval: TimeSpan.FromSeconds(1),
            timeout: TimeSpan.FromSeconds(10)
        );

        //Assert
        owner.Should().NotBeNull();
        owner.IsDeactivated.Should().BeTrue();

        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }

    [Fact]
    public async Task RabbitMQConsumer_ShouldMarkOwnerAsReactivated_WhenRecievedMessage()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        
        var ownerId = Guid.CreateVersion7();
        var deactivatedMessage= new UserDeactivatedMessage()
        {
            UserId = ownerId,
            TimeStamp = DateTime.UtcNow
        };
        var reactivatedMessage = new UserReactivatedMessage()
        {
            UserId = ownerId,
            TimeStamp = DateTime.UtcNow
        };


        //Act
        await SendMessage(channel, UserDeactivatedMessage.Topic, deactivatedMessage);

        await Eventually(
            func: () => _appContext.ProductOwners.AsNoTracking().FirstOrDefaultAsync(o => o.Id == ownerId && o.IsDeactivated),
            interval: TimeSpan.FromSeconds(1),
            timeout: TimeSpan.FromSeconds(10)
        );

        await SendMessage(channel, UserReactivatedMessage.Topic, reactivatedMessage);

        var owner = await Eventually(
            func: () => _appContext.ProductOwners.AsNoTracking().FirstOrDefaultAsync(o => o.Id == ownerId && !o.IsDeactivated),
            interval: TimeSpan.FromSeconds(1),
            timeout: TimeSpan.FromSeconds(10)
        );

        //Assert
        owner.Should().NotBeNull();
        owner.IsDeactivated.Should().BeFalse();

        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }

    [Fact]
    public async Task RabbitMQConsumer_ShouldMarkOwnerAsDeleted_WhenRecievedMessage()
    {
        //Arrange
        var connection = await _rabbitMQConnectionProvider.GetConnectionAsync(CancellationToken.None);
        var channel = await connection.CreateChannelAsync();
        
        var ownerId = Guid.CreateVersion7();
        var message = new UserDeletedMessage()
        {
            UserId = ownerId,
            TimeStamp = DateTime.UtcNow
        };


        //Act
        await SendMessage(channel, UserDeletedMessage.Topic, message);
        var owner = await Eventually(
            func: () => _appContext.ProductOwners.FirstOrDefaultAsync(o => o.Id == ownerId),
            interval: TimeSpan.FromSeconds(1),
            timeout: TimeSpan.FromSeconds(10)
        );

        //Assert
        owner.Should().NotBeNull();
        owner.IsDeleted.Should().BeTrue();

        try
        {
            await channel.CloseAsync();
        }
        catch (AlreadyClosedException) { }
    }

    private async Task SendMessage(IChannel channel, string topic, object notification)
    {
        if (channel is null) throw new ArgumentException();

        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);
        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: topic,
            mandatory: true,
            body: body
        );
    }

    private static async Task<T?> Eventually<T>(Func<Task<T?>> func, TimeSpan interval, TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;

        while (true)
        {
            var result = await func();

            if (result is not null) return result;

            if (DateTime.UtcNow - startTime > timeout) return default;

            await Task.Delay(interval);
        } 
    }
}