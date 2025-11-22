namespace ProductManagement.Infrastructure.Authentication.Configuration;

public class JwtOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Authority { get; set; } = null!;
}