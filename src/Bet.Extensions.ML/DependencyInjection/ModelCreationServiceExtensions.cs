using System;
using System.Linq;

using Bet.Extensions.ML;
using Bet.Extensions.ML.DataLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.Services;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelCreationServiceExtensions
    {
        public static IModelCreationServiceBuilder<TInput, TResult> AddModelCreationService<TInput, TResult>(this IServiceCollection services, string modelName)
            where TInput : class
            where TResult : MetricsResult
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MLContextOptions>, PostMLContextOptionsSetup>());

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

        public static IModelCreationServiceBuilder<TInput, TResult> AddSourceLoader<TInput, TResult, TLoader>(
            this IModelCreationServiceBuilder<TInput, TResult> builder,
            Action<SourceLoaderFileOptions<TInput>> configure,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TInput : class
                where TResult : MetricsResult
                where TLoader : SourceLoader<TInput>
        {
            builder.Services.AddSourceLoader<TInput, TLoader>(builder.ModelName, configure, serviceLifetime);
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
            builder.Services.AddModelLoader<TInput, TLoader>(builder.ModelName, configure, serviceLifetime);
            return builder;
        }

        public static IModelCreationServiceBuilder<TInput, TResult> ConfigureModelDefinition<TInput, TResult>(
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
                var mlContext = sp.GetRequiredService<IOptions<MLContextOptions>>();
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
