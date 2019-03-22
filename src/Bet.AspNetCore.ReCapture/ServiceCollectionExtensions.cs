using Bet.AspNetCore.ReCapture;
using Bet.AspNetCore.ReCapture.Google;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Google ReCapture Validation to your website.
        /// </summary>
        /// <param name="services">The default <see cref="IServiceCollection"/> for the application.</param>
        /// <param name="configuration">The default <see cref="IConfiguration"/> for the application.</param>
        /// <param name="validateConfigurations">The default value is true. Throws exception if the required values are absent.</param>
        /// <returns></returns>
        public static IServiceCollection AddReCapture(
            this IServiceCollection services,
            IConfiguration configuration,
            bool validateConfigurations = true)
        {
            if (validateConfigurations)
            {
                services.AddConfigurationValidation();

                services.ConfigureWithDataAnnotationsValidation<GoogleReCaptchaOptions>(configuration);
            }
            else
            {
                services.Configure<GoogleReCaptchaOptions>(configuration);
            }

            services.AddTransient<GoogleReCaptureFilter>();
            services.AddHttpClient<GoolgeReCaptureService>();

            return services;
        }
    }
}
