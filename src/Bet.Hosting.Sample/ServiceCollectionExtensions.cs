using System;

using Bet.Extensions.Hosting.Abstractions;
using Bet.Hosting.Sample.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModelBuilderService(this IServiceCollection services)
        {
            services.AddSpamDetectionModelBuilder();
            services.AddSentimentModelBuilder();

            services.AddTimedHostedService<ModelBuilderHostedService>(options =>
            {
                options.Interval = TimeSpan.FromMinutes(30);

                options.FailMode = FailMode.LogAndRetry;
                options.RetryInterval = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
