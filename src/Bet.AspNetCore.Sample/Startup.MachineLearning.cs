using System;

using Bet.AspNetCore.Sample;
using Bet.Extensions.ML.Azure.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelPredictionEngineExtensions
    {
        public static IServiceCollection AddSpamModelPrediction(
            this IServiceCollection services,
            IConfiguration configuration,
            string containerName = "models")
        {
            var isLocal = configuration.GetValue<bool>("LocalMLModels");
            var buildModels = configuration.GetValue<bool>("BuildModels");

            if (isLocal)
            {
                if (buildModels)
                {
                    // add sentiment model
                    services.AddSentimentModelCreationService<FileModelLoader>(
                        MLModels.SpamModel,
                        configure: options =>
                        {
                            options.ModelName = MLModels.SpamModel;
                            options.WatchForChanges = false;
                            options.ModelResultFileName = $"MLContent/{MLModels.SpamModel}.json";
                            options.ModelFileName = $"MLContent/{MLModels.SpamModel}.zip";
                        });
                }

                // add spam model from local ml.net models
                services.AddModelPredictionEngine<SpamInput, SpamPrediction>(
                    MLModels.SpamModel)
                    .From<SpamInput, SpamPrediction, FileModelLoader>(
                    options =>
                    {
                        options.ModelFileName = "MLContent/SpamModel.zip";
                        options.WatchForChanges = true;
                        options.ReloadInterval = TimeSpan.FromSeconds(50);
                    });
            }
            else
            {
                if (buildModels)
                {
                    // add sentiment model
                    services.AddSentimentModelCreationService<AzureStorageModelLoader>(MLModels.SpamModel);
                }

                // adds spam model used default azure configurations
                services.AddAzureStorageAccount(MLModels.SpamModel)
                        .AddAzureBlobContainer(MLModels.SpamModel, containerName);

                // register model loader with reload interval
                services.AddModelPredictionEngine<SpamInput, SpamPrediction>(MLModels.SpamModel)
                        .From<SpamInput, SpamPrediction, AzureStorageModelLoader>(options =>
                        {
                            options.WatchForChanges = true;
                            options.ReloadInterval = TimeSpan.FromSeconds(40);
                        });
            }

            return services;
        }

        public static IServiceCollection AddSentimentModelPrediction(
            this IServiceCollection services,
            IConfiguration configuration,
            string containerName = "models")
        {
            var isLocal = configuration.GetValue<bool>("LocalMLModels");
            var buildModels = configuration.GetValue<bool>("BuildModels");

            if (isLocal)
            {
                if (buildModels)
                {
                    // add sentiment model
                    services.AddSentimentModelCreationService<FileModelLoader>(
                        MLModels.SentimentModel,
                        configure: options =>
                        {
                            options.ModelName = MLModels.SentimentModel;
                            options.WatchForChanges = false;
                            options.ModelResultFileName = $"MLContent/{MLModels.SentimentModel}.json";
                            options.ModelFileName = $"MLContent/{MLModels.SentimentModel}.zip";
                        });
                }

                // add sentiment model
                services.AddModelPredictionEngine<SentimentIssue, SentimentPrediction>(MLModels.SentimentModel)
                    .From<SentimentIssue, SentimentPrediction, FileModelLoader>(options =>
                    {
                        options.ModelFileName = "MLContent/SentimentModel.zip";
                        options.WatchForChanges = true;
                        options.ReloadInterval = TimeSpan.FromSeconds(120);
                    });
            }
            else
            {
                if (buildModels)
                {
                    // add sentiment model
                    services.AddSentimentModelCreationService<AzureStorageModelLoader>(MLModels.SentimentModel);
                }

                // adds spam model used default azure configurations
                services.AddAzureStorageAccount(MLModels.SentimentModel)
                        .AddAzureBlobContainer(MLModels.SentimentModel, containerName);

                // register model loader with reload interval
                services.AddModelPredictionEngine<SentimentIssue, SentimentPrediction>(MLModels.SentimentModel)
                        .From<SentimentIssue, SentimentPrediction, AzureStorageModelLoader>(options =>
                        {
                            options.WatchForChanges = true;
                            options.ReloadInterval = TimeSpan.FromSeconds(60);
                        });
            }

            return services;
        }
    }
}
