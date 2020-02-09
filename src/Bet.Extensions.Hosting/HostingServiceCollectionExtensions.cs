using System;
using System.Collections.Generic;

using Bet.Extensions.Hosting.Hosted;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostingServiceCollectionExtensions
    {
        public static IServiceCollection AddTimedHostedService(
            this IServiceCollection services,
            string? serviceName,
            Action<TimedHostedServiceOptions> configure)
        {
            services.AddTimedHostedService<TimedHostedService>(serviceName ?? string.Empty, null, configure);

            return services;
        }

        public static IServiceCollection AddTimedHostedService<THostedService>(
            this IServiceCollection services,
            string? serviceName,
            string? sectionName,
            Action<TimedHostedServiceOptions> configure)
            where THostedService : TimedHostedService
        {
            var name = serviceName ?? string.Empty;

            services.AddChangeTokenOptions<TimedHostedServiceOptions>(
                sectionName ?? "TimedHostedService",
                name,
                options => configure?.Invoke(options));

            services.Add(ServiceDescriptor.Describe(
                typeof(IHostedService),
                sp =>
                {
                    var options = sp.GetRequiredService<IOptionsMonitor<TimedHostedServiceOptions>>().Get(serviceName);
                    var lifeHooks = sp.GetRequiredService<IEnumerable<ITimedHostedLifeCycleHook>>();
                    var logger = sp.GetRequiredService<ILogger<TimedHostedService>>();

                    return new TimedHostedService(sp, options, lifeHooks, logger);
                },
                ServiceLifetime.Singleton));

            return services;
        }
    }
}
