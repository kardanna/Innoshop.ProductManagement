using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.Infrastructure.Authentication.Configuration;

namespace ProductManagement.API.OptionsSetup;

public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme) return;

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        options.TokenHandlers.Clear();
        options.TokenHandlers.Add(handler);

        options.Authority = _jwtOptions.Authority;
        //options.MetadataAddress = $"{_jwtOptions.Authority}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true
        };
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
    }
}