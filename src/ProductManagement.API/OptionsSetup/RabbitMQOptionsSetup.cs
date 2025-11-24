using Microsoft.Extensions.Options;
using ProductManagement.Infrastructure.Messaging.Options;

namespace ProductManagement.API.OptionsSetup;

public class RabbitMQOptionsSetup : IConfigureOptions<RabbitMQOptions>
{
    private const string SECTION_NAME = "RabbitMQ";
    private readonly IConfiguration _configuration;

    public RabbitMQOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RabbitMQOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}