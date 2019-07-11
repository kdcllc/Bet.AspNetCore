using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpamDetectionModelBuilder(this IServiceCollection services)
        {
            services.TryAddSingleton(new MLContext());
            services.TryAddTransient<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>, SpamModelBuilder>();
            return services;
        }
    }
}
