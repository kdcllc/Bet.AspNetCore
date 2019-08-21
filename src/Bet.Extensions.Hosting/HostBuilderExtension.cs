using System.Linq;

using Bet.Extensions.Options;

using Microsoft.Extensions.DependencyInjection;

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
            hostBuilder.ConfigureServices((_, services) =>
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
