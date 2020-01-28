using System;

using Bet.Extensions.Hosting.Abstractions;
using Bet.Extensions.ML.Azure.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
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
        public static IServiceCollection AddMachineLearningHostedService(this IServiceCollection services)
        {
            services.AddMachineLearningModels();

            services.AddTimedHostedService<MachineLearningHostedService>(options =>
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
        public static IServiceCollection AddMachineLearningModels(this IServiceCollection services)
        {
            services.AddAzureStorageAccount("SpamModel1")
                            .AddAzureBlobContainer("SpamModel1", "models");

            return services
                        .AddSpamModelCreationService<AzureStorageModelManager>("SpamModel1", 0.2);
                        //.AddSpamModelCreationService<FileModelLoader>("SpamModel2", 0.5)
                        //.AddSentimentModelCreationService<FileModelLoader>();
        }
    }
}
