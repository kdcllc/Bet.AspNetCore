using System;

using Bet.AspNetCore.Middleware.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class DiagnosticsApplicationBuilder
    {
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
                throw new ArgumentException($"Please use {nameof(DiagnosticsServiceCollectionExtensions.AddDeveloperListRegisteredServices)} to configure {nameof(DeveloperListRegisteredServicesOptions)}");
            }

            return builder.UseMiddleware<DeveloperListRegisteredServicesMiddleware>();
        }

        /// <summary>
        /// Uses default logging configuration to log Http Request/Response.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configure">The ability to configure logger. The default is null.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestResponseLogging(
            this IApplicationBuilder builder,
            Action<RequestProfilerModel>? configure = null)
        {
            if ( configure == null)
            {
                var logger = builder.ApplicationServices.GetRequiredService<ILogger<RequestResponseLoggingMiddleware>>();

                Action<RequestProfilerModel> requestResponseHandler = requestProfilerModel =>
                {
                    logger.LogInformation(requestProfilerModel.Request);
                    logger.LogInformation(Environment.NewLine);
                    logger.LogInformation(requestProfilerModel.Response);
                };
                return builder.UseMiddleware<RequestResponseLoggingMiddleware>(requestResponseHandler);
            }

            return builder.UseMiddleware<RequestResponseLoggingMiddleware>(configure);
        }
    }
}
