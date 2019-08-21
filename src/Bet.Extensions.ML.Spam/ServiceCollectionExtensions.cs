using System;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
        /// <returns></returns>
        public static IServiceCollection AddSpamDetectionModelBuilder(this IServiceCollection services, IModelStorageProvider modelStorageProvider = null)
        {
            services.TryAddSingleton(new MLContext());

            if (modelStorageProvider == null)
            {
                modelStorageProvider = new FileModelStorageProvider();
            }

            services.TryAddScoped<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>, SpamModelBuilder>();

            services.AddScoped<IModelBuilderService, SpamModelBuilderService>((sp) =>
            {
                var builder = sp.GetRequiredService<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>>();
                var logger = sp.GetRequiredService<ILogger<SpamModelBuilderService>>();

                return new SpamModelBuilderService(builder, modelStorageProvider, logger);
            });

            return services;
        }
    }
}
