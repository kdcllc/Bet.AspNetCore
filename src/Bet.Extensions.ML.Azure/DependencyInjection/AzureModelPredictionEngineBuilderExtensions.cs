using System;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML.Azure.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureModelPredictionEngineBuilderExtensions
    {
        public static IModelPredictionEngineBuilder<TInput, TPrediction> FromAzureBlob<TInput, TPrediction>(
            this IModelPredictionEngineBuilder<TInput, TPrediction> builder,
            string blobContainerName,
            string sectionName = AzureStorageConstants.DefaultAccount,
            string rootSectionName = AzureStorageConstants.AzureStorage,
            Action<StorageAccountOptions>? configureStorage = null,
            Action<ModelLoderFileOptions>? configure = default) where TInput : class
         where TPrediction : class, new()
        {
            builder.Services.AddAzureStorageAccount(builder.ModelName, sectionName, rootSectionName, configureStorage)
                            .AddAzureBlobContainer(builder.ModelName, blobContainerName);

            builder.From<TInput, TPrediction, AzureStorageModelLoader>(configure);

            return builder;
        }
    }
}
