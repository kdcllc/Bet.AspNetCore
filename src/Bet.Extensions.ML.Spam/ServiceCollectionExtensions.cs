using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ML.NET based Spam detection model builder with <see cref="FileModelStorageProvider"/> as storage.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpamDetectionModelBuilder(this IServiceCollection services)
        {
            services.TryAddSingleton(new MLContext());
            services.TryAddTransient<IModelStorageProvider, FileModelStorageProvider>();
            services.TryAddTransient<IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>, SpamModelBuilder>();

            services.AddTransient<IModelBuilderService, SpamModelBuilderService>();

            return services;
        }
    }
}
