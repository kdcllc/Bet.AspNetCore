using System;
using System.Threading;

using Bet.Extensions.ML;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelPredictionEngineBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="IModelPredictionEngine{TInput, TPrediction}"/> based on the <see cref="ModelPoolLoader{TInput, TPrediction}"/> implementation.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TPrediction"></typeparam>
        /// <param name="services"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static IModelPredictionEngineBuilder<TInput, TPrediction> AddModelPredictionEngine<TInput, TPrediction>(
            this IServiceCollection services,
            string modelName = "")
                where TInput : class
                where TPrediction : class, new()
        {
            // enables with generic host
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MLContextOptions>, PostMLContextOptionsSetup>());

            services.AddSingleton<IModelPredictionEngine<TInput, TPrediction>, ModelPredictionEngine<TInput, TPrediction>>();

            return new ModelPredictionEngineBuilder<TInput, TPrediction>(services, modelName);
        }

        public static IModelPredictionEngineBuilder<TInput, TPrediction> From<TInput, TPrediction, TLoader>(
            this IModelPredictionEngineBuilder<TInput, TPrediction> builder,
            Action<ModelLoderFileOptions>? configure = default,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TInput : class
            where TPrediction : class, new()
            where TLoader : ModelLoader
        {
            builder.Services.AddModelLoader<TInput, TLoader>(builder.ModelName, configure, serviceLifetime);

            builder.Services
                    .AddOptions<ModelPredictionEngineOptions<TInput, TPrediction>>(builder.ModelName)
                    .Configure<IServiceProvider>(
                      (mlOptions, sp) =>
                      {
                          mlOptions.ModelName = builder.ModelName;

                          mlOptions.ModelLoader = sp.GetRequiredService<IOptionsMonitor<ModelLoaderOptions>>()
                                                                 .Get(builder.ModelName).ModalLoader;
                          mlOptions.ServiceProvider = sp;

                          mlOptions.CreateModel = (mlContext) =>
                          {
                              var model = mlOptions.ModelLoader.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();

                              return mlContext.Model.Load(model, out var inputSchema);
                          };
                      });

            return builder;
        }

        private static IModelPredictionEngineBuilder<TInput, TPrediction> AddModelLoader<TInput, TPrediction, TLoader>(
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
                    options.ModelName = builder.ModelName;
                    options.ModelResultFileName = $"{options.ModelName}.json";
                    options.ModelFileName = $"{options.ModelName}.zip";

                    // overrides the defaults
                    configure?.Invoke(options);
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
