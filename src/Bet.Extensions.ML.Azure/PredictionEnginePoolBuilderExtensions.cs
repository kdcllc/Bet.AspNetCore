using System;

using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML;
using Bet.Extensions.ML.Azure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.ML
{
    public static class PredictionEnginePoolBuilderExtensions
    {
        /// <summary>
        /// Adds Azure Blob Storage Provider with pooling checks for the updated models.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="builder"></param>
        /// <param name="modelName"></param>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="interval"></param>
        /// <param name="azureStorageSectionName"></param>
        /// <param name="setupStorage"></param>
        /// <returns></returns>
        public static PredictionEnginePoolBuilder<TData, TPrediction> FromAzureStorage<TData, TPrediction>(
            this PredictionEnginePoolBuilder<TData, TPrediction> builder,
            string modelName,
            string containerName,
            string fileName,
            TimeSpan interval,
            string azureStorageSectionName = "",
            Action<StorageAccountOptions>? setupStorage = null) where TData : class
           where TPrediction : class, new()
        {
            builder.Services.AddAzureStorage(azureStorageSectionName, setupStorage);

            builder.Services.TryAddTransient<AzureStorageModelLoader, AzureStorageModelLoader>();

            builder.Services.AddOptions<PredictionEnginePoolOptions<TData, TPrediction>>(modelName)
                .Configure<IServiceProvider, AzureStorageModelLoader>((options, sp, loader) =>
                {
                    var storageOptions = sp.GetRequiredService<IOptionsMonitor<StorageAccountOptions>>().Get(azureStorageSectionName);
                    var mlOptions = sp.GetRequiredService<IOptions<MLOptions>>().Value;
                    var logger = sp.GetRequiredService<ILogger<AzureBlobContainerLoader>>();

                    var containerLoader = new AzureBlobContainerLoader(containerName, mlOptions, storageOptions, logger);

                    loader.Start(containerLoader, fileName, interval);
                    options.ModelLoader = loader;
                });
            return builder;
        }
    }
}
