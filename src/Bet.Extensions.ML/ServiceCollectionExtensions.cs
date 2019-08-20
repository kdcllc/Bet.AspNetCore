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

            services.Configure(modelName, options);

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
        /// <typeparam name="TStorage"></typeparam>
        /// <param name="builder"></param>
        /// <param name="stroageName"></param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TData, TPrediction> WithStorageProvider<TData, TPrediction, TStorage>(
            this IModelPredictionEngineBuilder<TData, TPrediction> builder,
            string stroageName)
            where TData : class where TPrediction : class, new() where TStorage : class, IModelStorageProvider
        {
            builder.Services.AddSingleton(typeof(IModelStorageProvider), typeof(TStorage));

            builder.Services.Configure<ModelPredictionEngineOptions<TData, TPrediction>>(builder.ModelName, (mlOptions) =>
            {
                mlOptions.CreateModel = (mlContext) =>
                {
                    // var storage = mlOptions.ServiceProvider.GetService(typeof(TStorage)) as IModelStorageProvider;
                    var storage = mlOptions.ServiceProvider.GetRequiredService<IModelStorageProvider>();

                    ChangeToken.OnChange(
                        () => storage.GetReloadToken(),
                        () => mlOptions.Reload());

                    var model = storage.LoadModelAsync(stroageName, CancellationToken.None).GetAwaiter().GetResult();
                    return mlContext.Model.Load(model, out var inputSchema);
                };
            });

            return builder;
        }
    }
}
