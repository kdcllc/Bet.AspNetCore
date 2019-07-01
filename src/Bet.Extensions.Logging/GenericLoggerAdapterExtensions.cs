using System.Linq;

using Bet.Extensions.Logging;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GenericLoggerAdapterExtensions
    {
        /// <summary>
        /// Adds <see cref="GenericLoggerAdapter{T}"/> for specified interface type.
        /// </summary>
        /// <typeparam name="T">The type of the Interface to log against.</typeparam>
        /// <param name="services">The DI services.</param>
        /// <param name="logger">The instance of the generic logger.</param>
        /// <returns></returns>
        public static IServiceCollection AddGenericLogger<T>(this IServiceCollection services, ILogger logger)
            where T : class
        {
            if (!services.Any(x => x.ServiceType == typeof(ILogger)))
            {
                services.AddSingleton(logger);
            }

            services.AddSingleton<ILogger<T>, GenericLoggerAdapter<T>>();

            return services;
        }
    }
}
