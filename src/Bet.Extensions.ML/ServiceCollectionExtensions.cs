using Bet.Extensions.ML;
using Bet.Extensions.ML.Prediction;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
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
                        return mlContext.Model.Load(fileStream);
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
            Action<ModelPredictionEngineOptions> options,
            string modelName = Constants.MLDefaultModelName) where TData : class where TPrediction : class, new()
        {
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ModelPredictionEngineOptions>, PostModelPredictionEngineOptionsConfiguration>());

            services.Configure(modelName, options);

            services.AddSingleton<IModelPredictionEngine<TData, TPrediction>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PredictionEnginePooledObjectPolicy<TData, TPrediction>>>();
                var configuration = sp.GetRequiredService<IOptionsMonitor<ModelPredictionEngineOptions>>().Get(modelName);

                configuration.ModelName = modelName;

                return new ModelPredictionEngineObjectPool<TData, TPrediction>(configuration, logger);
            });

            return services;
        }
    }
}
