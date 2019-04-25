using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Bet.AspNetCore.Middleware.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="DeveloperListRegisteredServicesOptions"/> for the <see cref="DeveloperListRegisteredServicesMiddleware"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddDeveloperListRegisteredServices(
            this IServiceCollection services,
            Action<DeveloperListRegisteredServicesOptions> configure = null)
        {
            services.Configure<DeveloperListRegisteredServicesOptions>(c =>
            {
                c.Services = new List<ServiceDescriptor>(ImmutableList.CreateRange(services));
                configure?.Invoke(c);
            });

            return services;
        }

        /// <summary>
        /// Use <see cref="DeveloperListRegisteredServicesMiddleware"/> for listing all of the registered services within the DI.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDeveloperListRegisteredServices(this IApplicationBuilder builder)
        {
            var check = builder.ApplicationServices.GetService<IOptions<DeveloperListRegisteredServicesOptions>>();

            if (check?.Value?.Services == null)
            {
                throw new ArgumentException($"Please use {nameof(AddDeveloperListRegisteredServices)} to configure {nameof(DeveloperListRegisteredServicesOptions)}");
            }

            return builder.UseMiddleware<DeveloperListRegisteredServicesMiddleware>();
        }
    }
}
