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
        /// Configure <see cref="{TOptions}"/> with <see cref="DataAnnotationValidateOptions{TOptions}"/>.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The DI Services</param>
        /// <param name="configuration">The configuration section</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithDataAnnotationsValidation<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration) where TOptions : class, new()
        {
            services.Configure<TOptions>(configuration);

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            services.AddSingleton<IValidateOptions<TOptions>>(new DataAnnotationValidateOptions<TOptions>(Microsoft.Extensions.Options.Options.DefaultName));

            var section = (IConfigurationSection)configuration;

            ValidateAtStartup(services, typeof(TOptions), section.Path);

            return services;
        }

        /// <summary>
        /// Configure <see cref="{TOptions}"/> with <see cref="DataAnnotationValidateOptions{TOptions}"/>.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The DI services</param>
        /// <param name="sectionName">The configuration name for the section.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithDataAnnotationsValidation<TOptions>(
            this IServiceCollection services,
            string sectionName = null) where TOptions : class, new()
        {
            // get config section
            var config = GetConfigurationSection<TOptions>(services, sectionName);

            return services.ConfigureWithDataAnnotationsValidation<TOptions>(config);
        }

        /// <summary>
        /// Configure <see cref="{TOptions}"/> with a validation delegate.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="validation"></param>
        /// <param name="failureMessage"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureWithValidation<TOptions>(
            this IServiceCollection services,
            Func<TOptions, bool> validation,
            string failureMessage,
            string sectionName = null) where TOptions : class, new()
        {
            var configuration = GetConfigurationSection<TOptions>(services, sectionName);

            services.Configure<TOptions>(configuration);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            // validation
            services.AddSingleton<IValidateOptions<TOptions>>(new ValidateOptions<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, validation, failureMessage));

            var section = (IConfigurationSection)configuration;

            ValidateAtStartup(services, typeof(TOptions), section.Path);

            return services;
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

        private static IConfiguration GetConfigurationSection<TOptions>(IServiceCollection services, string sectionName)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            return configuration.GetSection(sectionName ?? nameof(TOptions));
        }
    }
}
