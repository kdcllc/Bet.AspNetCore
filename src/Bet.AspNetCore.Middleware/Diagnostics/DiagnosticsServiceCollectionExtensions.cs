using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Bet.AspNetCore.Middleware.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiagnosticsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="DeveloperListRegisteredServicesOptions"/> for the <see cref="DeveloperListRegisteredServicesMiddleware"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddDeveloperListRegisteredServices(
            this IServiceCollection services,
            Action<DeveloperListRegisteredServicesOptions>? configure = null)
        {
            services.Configure<DeveloperListRegisteredServicesOptions>(c =>
            {
                c.Services = new List<ServiceDescriptor>(ImmutableList.CreateRange(services));
                configure?.Invoke(c);
            });

            return services;
        }
    }
}
