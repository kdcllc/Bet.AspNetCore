using Bet.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtension
    {
        /// <summary>
        /// Enables validation of the Configurations on the startup.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseStartupFilter(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {

                var hostFilter = services.Select(x => x.ImplementationInstance).OfType<IValidationFilter>().FirstOrDefault();

                if (hostFilter == null)
                {
                    hostFilter = new OptionsValidationHostStartupFilter();
                    services.AddSingleton<IValidationFilter>(hostFilter as IValidationFilter);
                    services.AddSingleton<IHostStartupFilter>(hostFilter as IHostStartupFilter);
                }

                services.AddHostedService<HostStartupService>();
            });

            return hostBuilder;
        }
    }
}
