using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsValidationExtensions
    {
        /// <summary>
        /// Configure TOptions with <see cref="DataAnnotationValidateOptions{TOptions}"/>.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section for the Options.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithDataAnnotationsValidation<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration) where TOptions : class, new()
        {
            return services.Build(configuration, () => new DataAnnotationValidateOptions<TOptions>(Options.Options.DefaultName));
        }

        /// <summary>
        /// Configure TOptions with <see cref="DataAnnotationValidateOptions{TOptions}"/>.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section for the Options.</param>
        /// <param name="sectionName">The configuration name for the section. Default is null and the name equals {TOptions}.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithDataAnnotationsValidation<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = null) where TOptions : class, new()
        {
            // get config section
            var section = GetConfigurationSection<TOptions>(configuration, sectionName);

            return services.ConfigureWithDataAnnotationsValidation<TOptions>(section);
        }

        /// <summary>
        /// Configure TOptions with a validation delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> section for the Options.</param>
        /// <param name="validation">The validation delegate.</param>
        /// <param name="failureMessage">The message to be returned if validation delegation failed.</param>
        /// <param name="sectionName">The configuration name for the section. Default is null and the name equals {TOptions}.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithValidation<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<TOptions, bool> validation,
            string failureMessage,
            string sectionName = null) where TOptions : class, new()
        {
            var section = GetConfigurationSection<TOptions>(configuration, sectionName);

            return services.Build(section, () => new ValidateOptions<TOptions>(Options.Options.DefaultName, validation, failureMessage));
        }

        private static void ValidateAtStartup(
            IServiceCollection services,
            Type type,
            string sectionName)
        {
            var existingService = services.Select(x => x.ImplementationInstance).OfType<OptionsValidationStartupFilter>().FirstOrDefault();
            if (existingService == null)
            {
                existingService = new OptionsValidationStartupFilter();
                services.AddSingleton<IStartupFilter>(existingService);
            }
            existingService.OptionsTypes.Add((type, sectionName));
        }

        private static IServiceCollection Build<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<IValidateOptions<TOptions>> validator) where TOptions : class, new()
        {
            services.Configure<TOptions>(configuration);

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            services.AddSingleton(validator());

            var section = (IConfigurationSection)configuration;

            ValidateAtStartup(services, typeof(TOptions), section.Path);

            return services;
        }

        private static IConfiguration GetConfigurationSection<TOptions>(
            IConfiguration configuration,
            string sectionName)
        {
            return configuration.GetSection(sectionName ?? nameof(TOptions));
        }
    }
}
