using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Sentiment;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ML.NET based Sentiment model builder with <see cref="FileModelStorageProvider"/> as storage.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSentimentModelBuilder(this IServiceCollection services)
        {
            services.TryAddSingleton(new MLContext());

            services.TryAddTransient<IModelStorageProvider, FileModelStorageProvider>();
            services.TryAddTransient<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>, SentimentModelBuilder>();
            services.AddTransient<IModelBuilderService, SentimentModelBuilderService>();

            return services;
        }
    }
}
