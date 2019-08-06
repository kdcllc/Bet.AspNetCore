using System;
using System.IO;

using Bet.Extensions.ML;
using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

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
        public static IServiceCollection AddModelPredictionEngine<TData, TPrediction>(
            this IServiceCollection services,
            string mlModelPath,
            string modelName = Constants.MLDefaultModelName) where TData : class where TPrediction : class, new()
        {
            services.AddModelPredictionEngine<TData, TPrediction>(options =>
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

            return services;
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
        public static IServiceCollection AddModelPredictionEngine<TData,TPrediction>(
            this IServiceCollection services,
            Action<ModelPredictionEngineOptions<TData, TPrediction>> options,
            string modelName = Constants.MLDefaultModelName) where TData : class where TPrediction : class, new()
        {
            // enables with generic host
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.ConfigureOptions<ModelPredictionEngineSetup<TData,TPrediction>>();

            services.Configure(modelName,options);

            services.AddTransient<Func<ModelPredictionEngineOptions<TData,TPrediction>>>(provider => () =>
            {
                return provider.GetRequiredService<IOptionsMonitor<ModelPredictionEngineOptions<TData, TPrediction>>>().Get(modelName);
            });

            services.AddSingleton<IModelPredictionEngine<TData, TPrediction>,ModelPredictionEngineObjectPool<TData,TPrediction>>();

            return services;
        }
    }
}
