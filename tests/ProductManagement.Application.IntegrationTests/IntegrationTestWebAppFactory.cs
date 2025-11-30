using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProductManagement.API;
using ProductManagement.Infrastructure.Messaging.Options;
using ProductManagement.Persistence;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace ProductManagement.Application.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();
    
    private const string RabbitMQUsername = "test";
    private const string RabbitMQPassword = "test";
    private readonly RabbitMqContainer _rabbitMQContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.2.0-management")
        .WithUsername(RabbitMQUsername)
        .WithPassword(RabbitMQPassword)
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMQContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            ConfigureMSSqlServer(services);

            ConfigureRabbitMQ(services);            
        });
    }

    public async new Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _rabbitMQContainer.StopAsync();
    }

    private void ConfigureRabbitMQ(IServiceCollection services)
    {
        var rebbitMqOptionsDesctiptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(IConfigureOptions<RabbitMQOptions>));
        
        if (rebbitMqOptionsDesctiptor is not null)
        {
            services.Remove(rebbitMqOptionsDesctiptor);
        }

        var rabbitMqOptions = new RabbitMQOptions()
        {
            HostName = _rabbitMQContainer.Hostname,
            Port = _rabbitMQContainer.GetMappedPublicPort(5672),
            UserName = RabbitMQUsername,
            Password = RabbitMQPassword
        };
        
        services.AddSingleton<RabbitMQOptions>(rabbitMqOptions);
        services.AddSingleton<IOptions<RabbitMQOptions>>(sp =>
            Options.Create(sp.GetRequiredService<RabbitMQOptions>()));
    }

    private void ConfigureMSSqlServer(IServiceCollection services)
    {
        var dbContextDescriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ApplicationContext>));
        
        if (dbContextDescriptor is not null)
        {
            services.Remove(dbContextDescriptor);
        }

        services.AddDbContext<ApplicationContext>(options =>
        {
            options
                .UseSqlServer(
                    _dbContainer.GetConnectionString(),
                    contextOptions =>
                        {
                            contextOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null
                            );
                        });
        });

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var appContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        appContext.Database.Migrate();
    }
}