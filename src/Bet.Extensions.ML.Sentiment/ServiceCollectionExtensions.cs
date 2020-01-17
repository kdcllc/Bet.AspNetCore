using System;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Sentiment;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ML.NET based Sentiment model builder with <see cref="FileModelStorageProvider"/> as storage.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="modelStorageProvider">The model storage provider.</param>
        /// <param name="configure">The Model Builder configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddSentimentModelBuilder(
            this IServiceCollection services,
            IModelStorageProvider? modelStorageProvider = default,
            Action<SentimentModelBuilderServiceOptions>? configure = null)
        {
            services.TryAddSingleton(new MLContext());

            if (modelStorageProvider == null)
            {
                modelStorageProvider = new FileModelStorageProvider();
            }

            services.Configure<SentimentModelBuilderServiceOptions>(x => configure?.Invoke(x));

            services.TryAddScoped<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>, SentimentModelCreationBuilder>();

            services.AddScoped<IModelBuilderService, SentimentModelBuilderService>((sp) =>
            {
                var builder = sp.GetRequiredService<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>>();

                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<SentimentModelBuilderServiceOptions>>();

                var logger = sp.GetRequiredService<ILogger<SentimentModelBuilderService>>();

                return new SentimentModelBuilderService(builder, modelStorageProvider, optionsMonitor, logger);
            });

            return services;
        }
    }
}
