using System;

using Bet.Extensions.Hosting.Abstractions;
using Bet.Hosting.Sample.Services;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModelBuilderService(this IServiceCollection services)
        {
            services.AddScoped<IModelStorageProvider, FileModelStorageProvider>();
            services.TryAddSingleton(new MLContext());

            services.AddSpamDetectionModelBuilder();
            services.AddScoped<IModelBuilderService,SpamModelBuilderService>();

            services.AddSentimentModelBuilder();
            services.AddScoped<IModelBuilderService,SentimentModelBuilderService>();

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
