using System;
using System.IO;
using System.Threading;

using Bet.Extensions.ML;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelPredictionEngineBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="IModelPredictionEngine{TData, TPrediction}"/> based on the <see cref="ModelPredictionEngineObjectPool{TData, TPrediction}"/> implementation.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="mlModelPath">The path to the ML model file.</param>
        /// <param name="modelName">The Unique ML model name. The default <see cref="Constants.MLDefaultModelName"/>.</param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TInput, TPrediction> AddModelPredictionEngine<TInput, TPrediction>(
            this IServiceCollection services,
            string mlModelPath,
            string modelName)
                where TInput : class
                where TPrediction : class, new()
        {
            return services.AddModelPredictionEngine<TInput, TPrediction>(
                modelName,
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
                });
        }

        /// <summary>
        /// Adds <see cref="IModelPredictionEngine{TData, TPrediction}"/> based on the <see cref="ModelPredictionEngineObjectPool{TData, TPrediction}"/> implementation.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="modelName"></param>
        /// <param name="options">The Default values for the Named Machine Learning model.</param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TInput, TPrediction> AddModelPredictionEngine<TInput, TPrediction>(
            this IServiceCollection services,
            string modelName,
            Action<ModelPredictionEngineOptions<TInput, TPrediction>>? options = default)
                where TInput : class
                where TPrediction : class, new()
        {
            // enables with generic host
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.ConfigureOptions<ModelPredictionEngineSetup<TInput, TPrediction>>();

            if (options != null)
            {
                services.Configure(modelName, options);
            }

            services.AddSingleton<Func<ModelPredictionEngineOptions<TInput, TPrediction>>>(provider => () =>
             {
                 return provider.GetRequiredService<IOptionsMonitor<ModelPredictionEngineOptions<TInput, TPrediction>>>().Get(modelName);
             });

            services.AddSingleton<IModelPredictionEngine<TInput, TPrediction>, ModelPredictionEngineObjectPool<TInput, TPrediction>>();

            return new ModelPredictionEngineBuilder<TInput, TPrediction>(services, modelName);
        }

        public static IModelPredictionEngineBuilder<TInput, TPrediction> From<TInput, TPrediction, TLoader>(
            this IModelPredictionEngineBuilder<TInput, TPrediction> builder)
            where TInput : class
            where TPrediction : class, new()
            where TLoader : ModelLoader
        {
            builder.AddModelLoader<TInput, TPrediction, TLoader>();

            builder.Services
                    .AddOptions<ModelPredictionEngineOptions<TInput, TPrediction>>(builder.ModelName)
                    .Configure<IServiceProvider>(
                      (mlOptions, sp) =>
                      {
                          mlOptions.CreateModel = (mlContext) =>
                          {
                              var loader = sp.GetRequiredService<IOptionsMonitor<ModelLoaderOptions>>()
                                             .Get(builder.ModelName).ModalLoader;

                              ChangeToken.OnChange(
                               () => loader.GetReloadToken(),
                               () => mlOptions.Reload());

                              var model = loader.LoadModelAsync(CancellationToken.None).GetAwaiter().GetResult();
                              return mlContext.Model.Load(model, out var inputSchema);
                          };
                      });

            return builder;
        }

        public static IModelPredictionEngineBuilder<TInput, TPrediction> AddModelLoader<TInput, TPrediction, TLoader>(
           this IModelPredictionEngineBuilder<TInput, TPrediction> builder,
           Action<ModelLoderFileOptions>? configure = null,
           ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TInput : class
            where TPrediction : class, new()
            where TLoader : ModelLoader
        {
            // adds model loader to DI
            builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TLoader), typeof(TLoader), serviceLifetime));

            // adds configuration for model file and model result file
            builder.Services.Configure<ModelLoderFileOptions>(
                builder.ModelName,
                options =>
                {
                    if (configure == null)
                    {
                        options.ModelName = builder.ModelName;
                        options.ModelResultFileName = $"{options.ModelName}.json";
                        options.ModelFileName = $"{options.ModelName}.zip";
                    }
                    else
                    {
                        configure?.Invoke(options);
                    }
                });

            // adds model loader options to be used.
            builder.Services.AddOptions<ModelLoaderOptions>(builder.ModelName)
                            .Configure<IServiceProvider, TLoader>(
                            (options, sp, loader) =>
                            {
                                var setupOptions = sp.GetRequiredService<IOptionsMonitor<ModelLoderFileOptions>>().Get(builder.ModelName);
                                loader.Setup(setupOptions);

                                options.ModalLoader = loader;
                            });
            return builder;
        }
    }
}
