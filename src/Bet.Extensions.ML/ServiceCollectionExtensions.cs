using System;
using System.IO;
using System.Threading;
using Bet.Extensions.ML;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IModelPredictionEngine{TData, TPrediction}"/> based on the <see cref="ModelPredictionEngineObjectPool{TData, TPrediction}"/> implementation.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="mlModelPath">The path to the ML model file.</param>
        /// <param name="modelName">The Unique ML model name. The default <see cref="Constants.MLDefaultModelName"/>.</param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TData, TPrediction> AddModelPredictionEngine<TData, TPrediction>(
            this IServiceCollection services,
            string mlModelPath,
            string modelName = Constants.MLDefaultModelName) where TData : class where TPrediction : class, new()
        {
            return services.AddModelPredictionEngine<TData, TPrediction>(
                options =>
                {
                    options.CreateModel = (mlContext) =>
                    {
                        using (var fileStream = File.OpenRead(mlModelPath))
                        {
                            var context = mlContext.Model.Load(fileStream, out var modelInputSchema);
                            options.InputSchema = modelInputSchema;

                            return context;
                        }
                    };
                },
                modelName);
        }

        /// <summary>
        /// Adds <see cref="IModelPredictionEngine{TData, TPrediction}"/> based on the <see cref="ModelPredictionEngineObjectPool{TData, TPrediction}"/> implementation.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="options">The Default values for the Named Machine Learning model.</param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TData, TPrediction> AddModelPredictionEngine<TData, TPrediction>(
            this IServiceCollection services,
            Action<ModelPredictionEngineOptions<TData, TPrediction>> options = null,
            string modelName = Constants.MLDefaultModelName)
            where TData : class where TPrediction : class, new()
        {
            // enables with generic host
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.ConfigureOptions<ModelPredictionEngineSetup<TData, TPrediction>>();

            if (options != null)
            {
                services.Configure(modelName, options);
            }

            services.AddSingleton<Func<ModelPredictionEngineOptions<TData, TPrediction>>>(provider => () =>
             {
                 return provider.GetRequiredService<IOptionsMonitor<ModelPredictionEngineOptions<TData, TPrediction>>>().Get(modelName);
             });

            services.AddSingleton<IModelPredictionEngine<TData, TPrediction>, ModelPredictionEngineObjectPool<TData, TPrediction>>();

            return new DefaultModelPredictionEngineBuilder<TData, TPrediction>(services, modelName);
        }

        /// <summary>
        /// Enable model monitoring for the <see cref="IModelStorageProvider"/>.
        /// Overrides the default model storage provider.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="builder"></param>
        /// <param name="storageName">The name of the ML model in the storage provider.</param>
        /// <param name="modelStorageProvider">The model storage provider. The default model storage provider is <see cref="InMemoryModelStorageProvider"/>.</param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TData, TPrediction> WithStorageProvider<TData, TPrediction>(
            this IModelPredictionEngineBuilder<TData, TPrediction> builder,
            string storageName,
            IModelStorageProvider modelStorageProvider = null)
            where TData : class where TPrediction : class, new()
        {
            builder.Services.Configure(builder.ModelName, (Action<ModelPredictionEngineOptions<TData, TPrediction>>)((mlOptions) =>
            {
                mlOptions.CreateModel = (mlContext) =>
                {
                    if (modelStorageProvider == null)
                    {
                        modelStorageProvider = new InMemoryModelStorageProvider();
                    }

                    ChangeToken.OnChange(
                        () => modelStorageProvider.GetReloadToken(),
                        () => mlOptions.Reload());

                    return GetTransfomer(storageName, mlContext, modelStorageProvider);
                };
            }));

            return builder;
        }

        private static ITransformer GetTransfomer(string storageName, ML.MLContext mlContext, IModelStorageProvider storage)
        {
            var model = storage.LoadModelAsync(storageName, CancellationToken.None).GetAwaiter().GetResult();
            var transformer = mlContext.Model.Load(model, out var inputSchema);
            return transformer;
        }
    }
}
