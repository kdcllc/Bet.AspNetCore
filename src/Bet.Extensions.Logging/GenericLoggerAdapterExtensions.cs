
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
        /// <returns></returns>
        public static IServiceCollection AddGenericLogger<T>(this IServiceCollection services)
            where T : class
        {
            services.AddSingleton<ILogger<T>, GenericLoggerAdapter<T>>(sp=>
            {
                var logger = sp.GetRequiredService<ILogger<T>>();
                return new GenericLoggerAdapter<T>(logger);
             });

            return services;
        }
    }
}
