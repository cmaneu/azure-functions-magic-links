using Azure.Demos.MagicLinksAuth.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Azure.Demos.MagicLinksAuth.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // this will bind to the "Values" section of the configuration
            builder
                .Services
                .AddOptions<ApplicationSettings>()
                .Configure<IConfiguration>((settings, configuration) => { configuration.Bind(settings); });
        }
    }
}
