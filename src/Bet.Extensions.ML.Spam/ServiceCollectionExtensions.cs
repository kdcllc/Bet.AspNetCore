using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpamDetectionModelGenerator(this IServiceCollection services)
        {
            services.TryAddSingleton(new MLContext());
            services.TryAddScoped<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>,
                SpamModelBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>>();
            return services;
        }
    }
}
