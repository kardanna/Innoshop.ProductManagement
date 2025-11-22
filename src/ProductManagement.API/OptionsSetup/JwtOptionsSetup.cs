using Microsoft.Extensions.Options;
using ProductManagement.Infrastructure.Authentication.Configuration;

namespace ProductManagement.API.OptionsSetup;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private const string SECTION_NAME = "Jwt";
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}