using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FactoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Factory for the specify Implementation.
        /// </summary>
        /// <typeparam name="TService">The interface type to be used for the Factory.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="services">The DI services registration.</param>
        /// <returns></returns>
        public static IServiceCollection AddTransientFactory<TService, TImplementation>(this IServiceCollection services)
            where TService: class
            where TImplementation : class, TService
        {
            var registered = services.SingleOrDefault(sd => sd.ServiceType == typeof(TService))?.ImplementationType;
            if (registered != null)
            {
                throw new ArgumentException($"{typeof(TService)} has been registered with {registered.FullName}");
            }

            services.AddTransient<TService, TImplementation>();

            services.AddSingleton<Func<TService>>(sp => () =>
            {
                return sp.GetRequiredService<TService>();
            });

            services.AddSingleton<IFactory<TService>, Factory<TService>>();

            return services;
        }

        /// <summary>
        /// Add Transient Factory Selector Service.
        /// </summary>
        /// <typeparam name="TService">The interface type to be injected.</typeparam>
        /// <typeparam name="TSelector">The type of selector to be used for the DI resolve.</typeparam>
        /// <param name="services">The DI services.</param>
        /// <param name="selector">The type of selector.</param>
        /// <param name="implementationTypes">The possible return type implementations.</param>
        /// <returns></returns>
        public static IServiceCollection AddTransientFactorySelector<TService, TSelector>(
            this IServiceCollection services,
            Func<IServiceProvider, TSelector, TService> selector,
            params Type[] implementationTypes)
            where TService : class
        {
            foreach (var implType in implementationTypes)
            {
                services.AddTransient(implType);
            }

            services.AddSingleton<Func<IServiceProvider, TSelector, TService>>((sp, resolver) =>
            {
                return selector(sp, resolver);
            });

            services.AddSingleton<IFactorySelector<TSelector,TService>, FactorySelector<TSelector,TService>>();

            return services;
        }
    }
}
