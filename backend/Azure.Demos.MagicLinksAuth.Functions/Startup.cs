using Azure.Demos.MagicLinksAuth.Functions;
using Azure.Demos.MagicLinksAuth.Functions.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
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
            // TODO: Move audience and issuer to config
            builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        }
    }
}
