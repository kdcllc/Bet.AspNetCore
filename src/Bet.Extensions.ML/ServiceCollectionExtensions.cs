using Bet.Extensions.ML.Engine;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add <see cref="IMLModelEngine{TData, TPrediction}"/> prediction engine with specified model.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="modelFilePath"></param>
        /// <param name="maximumObjectRetained"></param>
        /// <returns></returns>
        public static IServiceCollection AddMLModelEngine<TData, TPrediction>(
            this IServiceCollection services,
            string modelFilePath,
            int maximumObjectRetained = -1) where TData : class where TPrediction : class, new()
        {
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.AddSingleton<IMLModelEngine<TData, TPrediction>>(sp =>
            {
                if (!File.Exists(modelFilePath))
                {
                    throw new ArgumentException($"File: {modelFilePath} doesn't exist");
                }

                var logger = sp.GetRequiredService<ILogger<PredictionEnginePooledObjectPolicy<TData, TPrediction>>>();

                return new MLModelEngineObjectPool<TData, TPrediction>(modelFilePath,logger,maximumObjectRetained);
            });

            return services;
        }
    }
}
