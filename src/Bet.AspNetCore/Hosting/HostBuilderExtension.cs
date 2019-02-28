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
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<HostStartupService>();
            });

            return hostBuilder;
        }
    }
}
