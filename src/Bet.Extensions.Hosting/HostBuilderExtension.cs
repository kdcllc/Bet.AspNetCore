using System.Linq;

using Bet.Extensions.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtension
    {
        /// <summary>
        /// Enables validation of the Configurations on the startup.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseStartupFilters(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((_, services) =>
            {
                var found = services.SingleOrDefault(x => x.ServiceType == typeof(IHostedService))?.ImplementationType == typeof(HostStartupService);
                if (!found)
                {
                    services.AddHostedService<HostStartupService>();
                }
            });

            return hostBuilder;
        }

        /// <summary>
        /// Enables validation of the Configurations on the startup.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseOptionValidation(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((_, services) =>
            {
                var found = services.SingleOrDefault(x => x.ServiceType == typeof(IHostedService))?.ImplementationType == typeof(HostStartupService);
                if (!found)
                {
                    services.AddHostedService<HostStartupService>();
                }

                var optionValidationFilter = services.Select(x => x.ImplementationInstance).OfType<IValidationFilter>().FirstOrDefault();

                if (optionValidationFilter == null)
                {
                    optionValidationFilter = new OptionsValidationHostStartupFilter();
                    services.TryAddSingleton(optionValidationFilter as IValidationFilter);
                    services.TryAddSingleton(optionValidationFilter as IHostStartupFilter);
                }
            });

            return hostBuilder;
        }
    }
}
