using System;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.ML
{
    public static class AzurePredictionEnginePoolBuilderExtensions
    {
        /// <summary>
        /// Adds Azure Blob Storage Provider with pooling checks for the updated models.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="builder"></param>
        /// <param name="modelName"></param>
        /// <param name="blobContainerName"></param>
        /// <param name="fileName"></param>
        /// <param name="interval"></param>
        /// <param name="sectionName"></param>
        /// <param name="rootSectionName"></param>
        /// <param name="setupStorage"></param>
        /// <returns></returns>
        public static PredictionEnginePoolBuilder<TData, TPrediction> FromAzureStorage<TData, TPrediction>(
            this PredictionEnginePoolBuilder<TData, TPrediction> builder,
            string modelName,
            string blobContainerName,
            string fileName,
            TimeSpan interval,
            string sectionName = AzureStorageConstants.DefaultAccount,
            string rootSectionName = AzureStorageConstants.AzureStorage,
            Action<StorageAccountOptions>? setupStorage = null) where TData : class
           where TPrediction : class, new()
        {
            builder.Services.AddAzureStorageAccount(modelName, sectionName, rootSectionName, setupStorage)
                            .AddAzureBlobContainer(modelName, blobContainerName);

            builder.Services.TryAddTransient<AzureStorageMSModelLoader, AzureStorageMSModelLoader>();

            builder.Services.AddOptions<PredictionEnginePoolOptions<TData, TPrediction>>(modelName)
                .Configure<AzureStorageMSModelLoader>((options, loader) =>
                {
                    loader.Start(modelName, fileName, interval);
                    options.ModelLoader = loader;
                });
            return builder;
        }
    }
}
