using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTimedHostedService(
            this IServiceCollection services,
            Func<CancellationToken, Task> execute,
            Action<TimedHostedServiceOptions>? configure = default)
        {
            services.AddTimedHostedService<DefaultTimedHostedService>(
                sp =>
                {
                    var options = sp.GetRequiredService<IOptionsMonitor<TimedHostedServiceOptions>>();
                    var lifeCycleHooks = sp.GetRequiredService<IEnumerable<ITimedHostedLifeCycleHook>>();
                    var logger = sp.GetRequiredService<ILogger<ITimedHostedService>>();
                    return new DefaultTimedHostedService(execute, options, lifeCycleHooks, logger);
                },
                configure);

            return services;
        }

        public static IServiceCollection AddTimedHostedService<THostedService>(
            this IServiceCollection services,
            Func<IServiceProvider, THostedService> implementationFactory,
            Action<TimedHostedServiceOptions>? configure = default) where THostedService : class, ITimedHostedService
        {
            services.Configure<TimedHostedServiceOptions>(options => configure?.Invoke(options));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ITimedHostedService>(implementationFactory));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService>(implementationFactory));

            return services;
        }

        public static IServiceCollection AddTimedHostedService<THostedService>(
            this IServiceCollection services,
            Action<TimedHostedServiceOptions>? configure = default) where THostedService : class, ITimedHostedService, IHostedService
        {
            services.Configure<TimedHostedServiceOptions>(options => configure?.Invoke(options));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ITimedHostedService, THostedService>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, THostedService>());

            return services;
        }
    }
}
