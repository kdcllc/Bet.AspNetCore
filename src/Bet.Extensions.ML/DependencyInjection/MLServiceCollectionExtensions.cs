using System;
using System.Collections.Generic;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MLServiceCollectionExtensions
    {
        public static IServiceCollection AddSourceLoader<TInput, TLoader>(
            this IServiceCollection services,
            string modelName,
            Action<SourceLoaderFileOptions<TInput>> configure,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TInput : class
                where TLoader : SourceLoader<TInput>
        {
            // adds source loader into DI
            services.TryAdd(ServiceDescriptor.Describe(typeof(TLoader), typeof(TLoader), serviceLifetime));

            // add file options configurations.
            services.Configure<SourceLoaderFileOptions<TInput>>(modelName, options => configure(options));

            // create source loader options
            services.AddOptions<SourceLoaderOptions<TInput>>(modelName)
                    .Configure<IServiceProvider, TLoader>(
                        (options, sp, loader) =>
                        {
                            loader.Setup(sp, modelName);
                            options.SourceLoader = loader;
                        });
            return services;
        }

        public static IServiceCollection AddModelLoader<TInput, TLoader>(
            this IServiceCollection services,
            string modelName,
            Action<ModelLoderFileOptions>? configure = null,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TInput : class
                where TLoader : ModelLoader
        {
            // adds model loader to DI
            services.TryAdd(ServiceDescriptor.Describe(typeof(TLoader), typeof(TLoader), serviceLifetime));

            // adds configuration for model file and model result file
            services.Configure<ModelLoderFileOptions>(
                modelName,
                options =>
                {
                    options.ModelName = modelName;
                    options.WatchForChanges = false;
                    options.ModelResultFileName = $"{options.ModelName}.json";
                    options.ModelFileName = $"{options.ModelName}.zip";

                    configure?.Invoke(options);
                });

            // adds model loader options to be used.
            services.AddOptions<ModelLoaderOptions>(modelName)
                    .Configure<IServiceProvider, TLoader>(
                    (options, sp, loader) =>
                    {
                        var setupOptions = sp.GetRequiredService<IOptionsMonitor<ModelLoderFileOptions>>().Get(modelName);
                        loader.Setup(setupOptions);

                        options.ModalLoader = loader;
                    });
            return services;
        }
    }
}
