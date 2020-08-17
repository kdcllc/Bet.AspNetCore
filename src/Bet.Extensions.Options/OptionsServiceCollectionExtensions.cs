using System;

using Bet.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentsOptions(
            this IServiceCollection services,
            string sectionName = nameof(Environments))
        {
            // required
            services.AddOptions();

            services.TryAddSingleton<IConfigureOptions<Environments>>(sp =>
            {
                return new ConfigureNamedOptions<Environments>(string.Empty, opt =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var section = config.GetSection(sectionName);

                    opt.Clear();

                    config.Bind(nameof(Environments), opt);
                });
            });

            return services;
        }

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
            Action<TOptions, IServiceProvider>? configureAction = default) where TOptions : class, new()
        {
            // configure changeable configurations
            services.RegisterInternal<TOptions>(sectionName, optionName);

            // create options instance from the configuration
            services.AddTransient((Func<IServiceProvider, IConfigureNamedOptions<TOptions>>)(sp =>
            {
                return new ConfigureNamedOptions<TOptions>(optionName, options =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);

                    configureAction?.Invoke(options, sp);
                });
            }));

            // Registers an IConfigureOptions<TOptions> action configurator. Being last it will bind from config source first
            // and run the customization afterwards
            services
                .AddOptions<TOptions>(optionName)
                .PostConfigure<IServiceProvider>((options, sp) => configureAction?.Invoke(options, sp));

            return services;
        }

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
            // configure changeable configurations
            services.RegisterInternal<TOptions>(sectionName, optionName);

            // create options instance from the configuration
            services.AddTransient((Func<IServiceProvider, IConfigureNamedOptions<TOptions>>)(sp =>
            {
                return new ConfigureNamedOptions<TOptions>(optionName, options =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);

                    configureAction?.Invoke(options);
                });
            }));

            // Registers an IConfigureOptions<TOptions> action configurator. Being last it will bind from config source first
            // and run the customization afterwards
            services
                .AddOptions<TOptions>(optionName)
                .Configure(options => configureAction?.Invoke(options));

            return services;
        }

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
                Action<TOptions, IConfiguration>? configureAction = default) where TOptions : class, new()
        {
            // configure changeable configurations
            services.RegisterInternal<TOptions>(sectionName, optionName);

            // create options instance from the configuration
            services.AddTransient((Func<IServiceProvider, IConfigureNamedOptions<TOptions>>)(sp =>
             {
                 return new ConfigureNamedOptions<TOptions>(optionName, options =>
                 {
                     var configuration = sp.GetRequiredService<IConfiguration>();
                     configuration.Bind(sectionName, options);

                     configureAction?.Invoke(options, configuration);
                 });
             }));

            // Registers an IConfigureOptions<TOptions> action configurator. Being last it will bind from config source first
            // and run the customization afterwards
            services
                .AddOptions<TOptions>(optionName)
                .Configure<IConfiguration>((options, configuration) => configureAction?.Invoke(options, configuration));

            return services;
        }

        private static void RegisterInternal<TOptions>(
            this IServiceCollection services,
            string sectionName,
            string? optionName = null)
            where TOptions : class, new()
        {
            if (optionName == null)
            {
                optionName = Options.Options.DefaultName;
            }

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
        }
    }
}
