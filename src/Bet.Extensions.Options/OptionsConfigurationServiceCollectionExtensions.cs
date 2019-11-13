using System;

using Bet.Extensions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a configuration instance which TOptions will bind against without passing <see cref="IConfiguration"/> into the registration.
        /// </summary>
        /// <typeparam name="TConfigureType"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="sectionName">The Configuration Section Name from where to retrieve the values from.</param>
        /// <returns></returns>
        public static IServiceCollection Configure<TConfigureType, TOptions>(
           this IServiceCollection services,
           string sectionName)
           where TConfigureType : class
           where TOptions : class, new()
        {
            return services.Configure<TConfigureType, TOptions>(sectionName, Options.Options.DefaultName, _ => { });
        }

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against without passing <see cref="IConfiguration"/> into registration.
        /// In addition adds the singleton of the {TOptions}.
        /// https://github.com/aspnet/Extensions/blob/299af9e32ba790dbfe8cfdf99b441766d7b0f6b6/src/Options/ConfigurationExtensions/src/OptionsConfigurationServiceCollectionExtensions.cs#L58.
        /// </summary>
        /// <typeparam name="TConfigureType">The type of the object that configuration provider has entry for.</typeparam>
        /// <typeparam name="TOptions">The type of the option object.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="sectionName">The Configuration Section Name from where to retrieve the values from.</param>
        /// <param name="optionName">The named option name.</param>
        /// <param name="configureBinder">The <see cref="ConfigurationBinder"/>.</param>
        /// <returns></returns>
        public static IServiceCollection Configure<TConfigureType, TOptions>(
            this IServiceCollection services,
            string sectionName,
            string optionName,
            Action<BinderOptions> configureBinder)
            where TConfigureType : class
            where TOptions : class, new()
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            services.AddOptions();

            services.AddSingleton<IOptionsChangeTokenSource<TOptions>>((sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var section = config.GetSection(sectionName).GetSection(typeof(TConfigureType).Name);

                return new ConfigurationChangeTokenSource<TOptions>(optionName, section);
            });

            services.AddSingleton<IConfigureOptions<TOptions>>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var section = config.GetSection(sectionName).GetSection(typeof(TConfigureType).Name);

                return new NamedConfigureFromConfigurationOptions<TOptions>(optionName, section, configureBinder);
            });

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            return services;
        }

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IServiceCollection Configure<TOptions>(
            this IServiceCollection services,
            string? sectionName = default) where TOptions : class, new()
        {
            return services.Configure<TOptions>(_ => { }, sectionName);
        }

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureBinder">Used to configure the <see cref="BinderOptions"/>.</param>
        /// <param name="sectionName">The section name that is different from {TOptions}.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection Configure<TOptions>(
            this IServiceCollection services,
            Action<BinderOptions> configureBinder,
            string? sectionName = default)
            where TOptions : class, new()
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.AddSingleton<IOptionsChangeTokenSource<TOptions>>((sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var section = config.GetSection(sectionName ?? nameof(TOptions));

                return new ConfigurationChangeTokenSource<TOptions>(sectionName, section);
            });

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            return services.AddSingleton<IConfigureOptions<TOptions>>((sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var section = config.GetSection(sectionName ?? nameof(TOptions));

                return new NamedConfigureFromConfigurationOptions<TOptions>(sectionName, config, configureBinder);
            });
        }

        /// <summary>
        /// Adds binding configuration callback for TOptions based on the configuration section.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options to be configured.</typeparam>
        /// <param name="services">The services.</param>
        /// <param name="sectionName">The configuration section to be used for this TOptions.</param>
        /// <param name="configure">The delegate to be used to configure the binding of the options.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureOptions<TOptions>(
            this IServiceCollection services,
            string sectionName,
            Action<IConfiguration, string, TOptions> configure) where TOptions : class
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var rootSection = sectionName ?? typeof(TOptions).Name;

            services.AddSingleton<IConfigureOptions<TOptions>>(sp => new OptionsConfiguration<TOptions>(sp.GetService<IConfiguration>(), rootSection, configure));
            return services;
        }
    }
}
