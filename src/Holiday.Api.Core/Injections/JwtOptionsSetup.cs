using Holiday.Api.Core.Configurations;
using Microsoft.Extensions.Options;

namespace Holiday.Api.Core.Injections;

public class JwtOptionsSetup : IConfigureOptions<JwtConfiguration>
{
    public static readonly string SectionName = "JwtSettings";
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtConfiguration options)
    {
        _configuration
            .GetSection(SectionName)
            .Bind(options);
    }
}