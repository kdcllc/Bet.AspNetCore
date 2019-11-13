using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Options that support being updated during application run.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <param name="optionName"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddChangeTokenOptions<TOptions>(
            this IServiceCollection services,
            string sectionName,
            string? optionName = default,
            Action<TOptions>? configureAction = default) where TOptions : class, new()
        {
            if (optionName == null)
            {
                optionName = Options.Options.DefaultName;
            }

            // create options instance from the configuration
            services.AddTransient<IConfigureNamedOptions<TOptions>>(sp =>
            {
                return new ConfigureNamedOptions<TOptions>(optionName, options =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);

                    configureAction?.Invoke(options);
                });
            });

            // configure changeable configurations
            services.AddSingleton((Func<IServiceProvider, IOptionsChangeTokenSource<TOptions>>)((sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>().GetSection(sectionName);
                return new ConfigurationChangeTokenSource<TOptions>(optionName, config);
            }));

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            services.AddSingleton((Func<IServiceProvider, IConfigureOptions<TOptions>>)(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>().GetSection(sectionName);
                return new NamedConfigureFromConfigurationOptions<TOptions>(optionName, config);
            }));

            // Registers an IConfigureOptions<TOptions> action configurator. Being last it will bind from config source first
            // and run the customization afterwards
            services.Configure<TOptions>(optionName, options => configureAction?.Invoke(options));

            return services;
        }
    }
}
