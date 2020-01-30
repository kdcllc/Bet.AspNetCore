using System.Collections.Generic;

using Bet.AspNetCore.Sample;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksExtensions
    {
        public static IServiceCollection AddAppHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var isLocal = configuration.GetValue<bool>("LocalMLModels");

            var builder = services.AddHealthChecks();

            if (!isLocal)
            {
                // blob hc check
                builder.AddAzureBlobStorageCheck("betstorage_check", "models", options =>
                {
                    options.Name = "betstorage";
                })

               // queue hc check
               .AddAzureQueuetorageCheck("queue_check", "betqueue");
            }

            // memory hc check
            builder.AddMemoryHealthCheck()

               // spam hc check
               .AddMachineLearningModelCheck<SpamInput, SpamPrediction>(
               $"{MLModels.SpamModel}_check",
               options =>
               {
                   options.ModelName = MLModels.SpamModel;
                   options.SampleData = new SpamInput
                   {
                       Message = "That's a great idea. It should work."
                   };
               })

               // sentiment hc check
               .AddMachineLearningModelCheck<SentimentIssue, SentimentPrediction>(
               $"{MLModels.SentimentModel}_check",
               options =>
               {
                   options.ModelName = MLModels.SentimentModel;
                   options.SampleData = new SentimentIssue
                   {
                       Text = "This is a very rude movie"
                   };
               })

               .AddSigtermCheck("sigterm_check")

               .AddLoggerPublisher(new List<string> { "sigterm_check" });

            return services;
        }
    }
}
