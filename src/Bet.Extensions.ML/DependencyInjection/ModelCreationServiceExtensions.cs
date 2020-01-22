using System;
using System.Linq;

using Bet.Extensions.ML.DataLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.Services;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelCreationServiceExtensions
    {
        public static IModelCreationServiceBuilder<TInput, TResult> AddModelCreationService<TInput, TResult>(this IServiceCollection services, string modelName)
            where TInput : class
            where TResult : MetricsResult
        {
            services.TryAddSingleton(new MLContext());

            services.AddOptions();

            // name of the model can change but the same types are applied, so filter by types
            if (!services.Any(d => d.ImplementationType == typeof(ModelCreationEngine<TInput, TResult, ModelCreationEngineOptions<TInput, TResult>>)))
            {
                services.AddTransient<IModelCreationEngine, ModelCreationEngine<TInput, TResult, ModelCreationEngineOptions<TInput, TResult>>>();
            }

            services.TryAddScoped<IModelCreationService, ModelCreationService>();

            services.TryAddSingleton<InMemoryStorage, InMemoryStorage>();

            return new ModelCreationServiceBuilder<TInput, TResult>(services, modelName);
        }

        public static IModelCreationServiceBuilder<TInput, TResult> AddSources<TInput, TResult, TLoader>(
            this IModelCreationServiceBuilder<TInput, TResult> builder,
            Action<SourceLoaderFileOptions<TInput>> configure,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TInput : class
                where TResult : MetricsResult
                where TLoader : SourceLoader<TInput>
        {
            // adds source loader into DI
            builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TLoader), typeof(TLoader), serviceLifetime));

            // add file options configurations.
            builder.Services.Configure<SourceLoaderFileOptions<TInput>>(builder.ModelName, options => configure(options));

            // create source loader options
            builder.Services.AddOptions<SourceLoaderOptions<TInput>>(builder.ModelName)
                            .Configure<IServiceProvider, TLoader>(
                                (options, sp, loader) =>
                                {
                                    var setupOptions = sp.GetRequiredService<IOptionsMonitor<SourceLoaderFileOptions<TInput>>>().Get(builder.ModelName);
                                    loader.Setup(setupOptions);
                                    options.SourceLoader = loader;
                                });
            return builder;
        }

        public static IModelCreationServiceBuilder<TInput, TResult> AddModelLoader<TInput, TResult, TLoader>(
            this IModelCreationServiceBuilder<TInput, TResult> builder,
            Action<ModelLoderFileOptions>? configure = null,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TInput : class
                where TResult : MetricsResult
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

        public static IModelCreationServiceBuilder<TInput, TResult> ConfigureModel<TInput, TResult>(
            this IModelCreationServiceBuilder<TInput, TResult> builder,
            double testSlipFraction,
            Action<ModelDefinitionBuilderOptions<TResult>> configureModel)
            where TInput : class
            where TResult : MetricsResult
        {
            // configured options for model definition
            builder.Services.AddOptions<ModelDefinitionBuilderOptions<TResult>>(builder.ModelName)
                            .Configure(
                            options =>
                            {
                                options.ModelName = builder.ModelName;
                                options.TestSlipFraction = testSlipFraction;

                                configureModel(options);
                            });

            // allows to add multiple instances of the typed object
            builder.Services.AddTransient<IModelDefinitionBuilder<TInput, TResult>>(sp =>
            {
                var mlContext = sp.GetRequiredService<MLContext>();
                var options = sp.GetRequiredService<IOptionsMonitor<ModelDefinitionBuilderOptions<TResult>>>().Get(builder.ModelName);
                var logger = sp.GetRequiredService<ILogger<ModelDefinitionBuilder<TInput, TResult>>>();

                return new ModelDefinitionBuilder<TInput, TResult>(mlContext, options, logger);
            });

            return builder;
        }

        public static IModelCreationServiceBuilder<TInput, TResult> ConfigureService<TInput, TResult>(
            this IModelCreationServiceBuilder<TInput, TResult> builder,
            Action<ModelCreationEngineOptions<TInput, TResult>> configureEngine)
            where TInput : class
            where TResult : MetricsResult
        {
            builder.Services.AddOptions<ModelCreationEngineOptions<TInput, TResult>>(builder.ModelName)
                            .Configure(
                            options =>
                            {
                                options.ModelName = builder.ModelName;

                                configureEngine?.Invoke(options);
                            });

            return builder;
        }
    }
}
