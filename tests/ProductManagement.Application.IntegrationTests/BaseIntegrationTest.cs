using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using ProductManagement.Persistence;

namespace ProductManagement.Application.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly ISender _sender;
    protected readonly ApplicationContext _appContext;
    protected readonly IRabbitMQConnectionProvider _rabbitMQConnectionProvider;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        _appContext = _scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        _rabbitMQConnectionProvider = _scope.ServiceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
    }
}