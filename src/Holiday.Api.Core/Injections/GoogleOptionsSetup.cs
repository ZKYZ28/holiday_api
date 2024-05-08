using Holiday.Api.Core.Configurations;
using Microsoft.Extensions.Options;

namespace Holiday.Api.Core.Injections;

public class GoogleOptionsSetup : IConfigureOptions<GoogleConfiguration>
{

    private readonly IConfiguration _configuration;

    public GoogleOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void Configure(GoogleConfiguration options)
    {
        _configuration
            .GetSection(ExternalServicesConfiguration.Section)
            .GetSection(ExternalServicesConfiguration.GoogleSubSection)
            .Bind(options);
    }
}