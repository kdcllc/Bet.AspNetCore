using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSentimentModelGenerator(this IServiceCollection services)
        {
            services.TryAddSingleton(new MLContext());
            services.TryAddTransient<IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>, SentimentModelBuilder>();
            return services;
        }
    }
}
