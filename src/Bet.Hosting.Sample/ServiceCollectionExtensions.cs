using System;

using Bet.Extensions.Hosting.Abstractions;
using Bet.Hosting.Sample.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds timed service, that can be run in a Docker container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddModelBuildersTimedService(this IServiceCollection services)
        {
            services.AddModelBuildersCronJobService();

            services.AddTimedHostedService<ModelBuilderHostedService>(options =>
            {
                options.Interval = TimeSpan.FromMinutes(30);

                options.FailMode = FailMode.LogAndRetry;
                options.RetryInterval = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        /// <summary>
        /// Adds this as Kubernetes CronJob.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddModelBuildersCronJobService(this IServiceCollection services)
        {
            return services.AddSpamDetectionModelBuilder()
                          .AddSentimentModelBuilder()
                          .AddScoped<IModelBuildersJobService, ModelBuildersJobService>();
        }
    }
}
