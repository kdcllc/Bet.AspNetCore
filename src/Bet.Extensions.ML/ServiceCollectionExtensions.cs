using Bet.Extensions.ML;
using Bet.Extensions.ML.Prediction;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        ///// <summary>
        ///// Add <see cref="IModelPredictionEngine{TData, TPrediction}"/> prediction engine with specified model.
        ///// </summary>
        ///// <typeparam name="TData"></typeparam>
        ///// <typeparam name="TPrediction"></typeparam>
        ///// <param name="services"></param>
        ///// <param name="modelFilePath"></param>
        ///// <param name="maximumObjectRetained"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddMLModelEngine<TData, TPrediction>(
        //    this IServiceCollection services,
        //    string modelFilePath,
        //    int maximumObjectRetained = -1) where TData : class where TPrediction : class, new()
        //{
        //    services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        //    services.AddSingleton<IModelPredictionEngine<TData, TPrediction>>(sp =>
        //    {
        //        if (!File.Exists(modelFilePath))
        //        {
        //            throw new ArgumentException($"File: {modelFilePath} doesn't exist");
        //        }

        //        var logger = sp.GetRequiredService<ILogger<PredictionEnginePooledObjectPolicy<TData, TPrediction>>>();

        //        return new ModelPredictionEngineObjectPool<TData, TPrediction>(modelFilePath,logger,maximumObjectRetained);
        //    });

        //    return services;
        //}

        public static IServiceCollection ModelPredictionEngine<TData,TPrediction>(
            this IServiceCollection services,
            Action<ModelPredictionEngineOptions> configure,
            string modelName=Constants.MLDefaultModelName) where TData : class where TPrediction : class, new()
        {
            services.Configure(modelName, configure);

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            return services;
        }
    }
}
