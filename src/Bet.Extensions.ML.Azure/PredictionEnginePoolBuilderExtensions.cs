using System;

using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML;

using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="setupStorage"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static PredictionEnginePoolBuilder<TData, TPrediction> FromAzureStorage<TData, TPrediction>(
            this PredictionEnginePoolBuilder<TData, TPrediction> builder,
            string modelName,
            string containerName,
            string fileName,
            Action<StorageAccountOptions> setupStorage,
            TimeSpan interval) where TData : class
           where TPrediction : class, new()
        {
            builder.Services.Configure<StorageAccountOptions>(modelName, o => setupStorage?.Invoke(o));
            builder.Services.ConfigureOptions<StorageAccountOptionsSetup>();

            builder.Services.AddTransient<AzureStorageModelLoader, AzureStorageModelLoader>();
            builder.Services.AddOptions<PredictionEnginePoolOptions<TData, TPrediction>>(modelName)
                .Configure<AzureStorageModelLoader, IOptionsMonitor<StorageAccountOptions>>((opt, loader, monitor) =>
                {
                    var storageOptions = monitor.Get(modelName);

                    loader.Start(storageOptions, containerName, fileName, interval);
                    opt.ModelLoader = loader;
                });
            return builder;
        }
    }
}
