using System;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ML.NET based Spam detection model builder with <see cref="FileModelStorageProvider"/> as storage.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="modelStorageProvider">The model storage provider.</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpamDetectionModelBuilder(
            this IServiceCollection services,
            IModelStorageProvider? modelStorageProvider = default,
            Action<SpamModelBuilderServiceOptions>? configure = null)
        {
            services.TryAddSingleton(new MLContext());

            if (modelStorageProvider == null)
            {
                modelStorageProvider = new FileModelStorageProvider();
            }

            services.TryAddScoped<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>, SpamModelCreationBuilder>();
            services.Configure<SpamModelBuilderServiceOptions>(x => configure?.Invoke(x));

            services.AddScoped<IModelBuilderService, SpamModelBuilderService>((sp) =>
            {
                var builder = sp.GetRequiredService<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>>();

                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<SpamModelBuilderServiceOptions>>();

                var logger = sp.GetRequiredService<ILogger<SpamModelBuilderService>>();

                return new SpamModelBuilderService(builder, modelStorageProvider, optionsMonitor, logger);
            });

            return services;
        }
    }
}
