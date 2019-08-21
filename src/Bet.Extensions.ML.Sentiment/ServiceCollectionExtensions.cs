using System;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Sentiment;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
        /// <returns></returns>
        public static IServiceCollection AddSentimentModelBuilder(this IServiceCollection services, IModelStorageProvider modelStorageProvider = null)
        {
            services.TryAddSingleton(new MLContext());

            if (modelStorageProvider == null)
            {
                modelStorageProvider = new FileModelStorageProvider();
            }

            services.TryAddScoped<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>, SentimentModelBuilder>();
            services.AddScoped<IModelBuilderService, SentimentModelBuilderService>((sp) =>
            {
                var builder = sp.GetRequiredService<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>>();
                var logger = sp.GetRequiredService<ILogger<SentimentModelBuilderService>>();

                return new SentimentModelBuilderService(builder, modelStorageProvider, logger);
            });

            return services;
        }
    }
}
