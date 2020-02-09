using System;

using Bet.Extensions.Hosting.Hosted;
using Bet.Extensions.ML.Azure.ModelLoaders;
using Bet.Extensions.ML.ModelCreation.Services;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BetHosingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds timed service, that can be run in a Docker container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMachineLearningHostedService(this IServiceCollection services)
        {
            services.AddMachineLearningModels();

            services.AddTimedHostedService(
            "MachineLearningService",
            options =>
            {
                options.TaskToExecuteAsync = async (opt, sp, cancellationToken) =>
                {
                    var job = sp.GetRequiredService<IModelCreationService>();
                    var logger = sp.GetRequiredService<ILogger<TimedHostedService>>();

                    try
                    {
                        await job.BuildModelsAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("{serviceName} failed with exception: {message}", nameof(TimedHostedService), ex.Message);
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Adds this as Kubernetes CronJob that builds <see cref="SpamModelEngineExtensions"/>
        /// and <see cref="SentimentModelEngineExtensions"/> ML.NET models.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMachineLearningModels(this IServiceCollection services)
        {
            const string containerName = "models";

            // model name is what connects everything together.
            var spamModelName = "SpamModel";

            services.AddAzureStorageAccount(spamModelName)
                    .AddAzureBlobContainer(spamModelName, containerName);

            services.AddSpamModelCreationService<AzureStorageModelLoader>(spamModelName, 0.2);

            // example of adding file based model generation
            // services.AddSpamModelCreationService<FileModelLoader>("SpamModel2", 0.5);
            // services.AddSpamModelCreationService<InMemoryModelLoader>("SpamModel1");
            var sentimentModelName = "SentimentModel";
            services.AddAzureStorageAccount(sentimentModelName)
                    .AddAzureBlobContainer(sentimentModelName, containerName);

            services.AddSentimentModelCreationService<AzureStorageModelLoader>(sentimentModelName);

            return services;
        }
    }
}
