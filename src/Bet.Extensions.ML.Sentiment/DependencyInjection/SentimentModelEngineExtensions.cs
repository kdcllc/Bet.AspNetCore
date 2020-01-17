using System.Collections.Generic;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.DataLoaders;
using Bet.Extensions.ML.ModelCreation.Results;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SentimentModelEngineExtensions
    {
        public static IServiceCollection AddSentimentModelEngine(this IServiceCollection services)
        {
            // adds embedded loader
            services.TryAddTransient<ISourceLoader<SentimentIssue>, EmbeddedSourceLoader<SentimentIssue>>();

            // configure the objects to get the data
            services.Configure<EmbeddedSourceLoaderOptions<SentimentIssue>>(options =>
            {
                options.EmbeddedSourcesList.Add(new EmbeddedSources<SentimentIssue>
                {
                    Overrides = () =>
                    {
                        var inputs = LoadFromEmbededResource
                        .GetRecords<InputSentimentIssueRow>("Content.wikiDetoxAnnotated40kRows.tsv", delimiter: "\t", hasHeaderRecord: true);

                        // convert int to boolean values
                        var result = new List<SentimentIssue>();
                        foreach (var item in inputs)
                        {
                            var newItem = new SentimentIssue
                            {
                                Label = item.Label != 0,
                                Text = item.comment
                            };

                            result.Add(newItem);
                        }

                        return result;
                    }
                });
            });

            // adds model creation engine
            services.TryAddScoped<IModelBuilder<SentimentIssue, BinaryClassificationMetricsResult>, ModelBuilder<SentimentIssue, BinaryClassificationMetricsResult>>();

            services.Configure<ModelBuilderOptions<BinaryClassificationMetricsResult>>(options =>
            {
                options.TrainingPipelineConfigurator = (mlContext) =>
                {
                    // STEP 2: Common data process configuration with pipeline data transformations
                    var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

                    // STEP 3: Set the training algorithm, then create and config the modelBuilder
                    var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                    var trainingPipeline = dataProcessPipeline.Append(trainer);

                    return new TrainingPipelineResult(trainingPipeline, trainer.ToString());
                };

                options.EvaluateConfigurator = (mlContext, model, trainerName, dataView, _) =>
                {
                    // STEP 5: Evaluate the model and show accuracy stats
                    var predictions = model.Transform(dataView);
                    var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

                    return new BinaryClassificationMetricsResult(trainerName, metrics);
                };
            });

            return services;
        }
    }
}
