using System;
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
        public static IModelCreationServiceBuilder AddModelCreationService<TInput, TResult>(this IServiceCollection services, string modelName)
            where TInput : class
            where TResult : MetricsResult
        {
            services.TryAddSingleton(new MLContext());

            services.AddOptions();

            services.TryAddTransient<IModelCreationEngine, ModelCreationEngine<TInput, TResult, ModelCreationEngineOptions<TInput, TResult>>>();

            services.TryAddScoped<IModelCreationService, ModelCreationService>();

            services.TryAddSingleton<InMemoryModelLoaderStorage, InMemoryModelLoaderStorage>();

            return new ModelCreationServiceBuilder(services, modelName);
        }

        public static IModelCreationServiceBuilder AddSources<TInput, TLoader>(
            this IModelCreationServiceBuilder builder,
            Action<SourceLoaderFileOptions<TInput>> configure,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TInput : class
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

        public static IModelCreationServiceBuilder AddModelLoader<TLoader>(
            this IModelCreationServiceBuilder builder,
            Action<ModelLoderFileOptions>? configure = null,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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

        public static IModelCreationServiceBuilder ConfigureModel<TInput, TResult>(
            this IModelCreationServiceBuilder builder,
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

        public static IModelCreationServiceBuilder ConfigureService<TInput, TResult>(
            this IModelCreationServiceBuilder builder,
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
