using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelCreationEngineOptions<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        public string ModelName { get; set; } = string.Empty;

        public Func<SourceLoader<TInput>, CancellationToken, Task<IEnumerable<TInput>>> DataLoader { get; set; }
            = async (sourceLoader, cancellationToken) =>
        {
            return await Task.Run(
                () => sourceLoader.LoadData(),
                cancellationToken);
        };

        /// <summary>
        /// Configures default ML.NET Train Model configurator.
        /// </summary>
        public Func<IModelDefinitionBuilder<TInput, TResult>, IEnumerable<TInput>, ILogger, TResult> TrainModelConfigurator { get; set; }
            = (modelBuilder, data, logger) =>
        {
            // 1. load ML data set
            modelBuilder.LoadData(data);

            // 2. build data view
            modelBuilder.BuildDataView();

            // 3. build training pipeline
            var buildTrainingPipelineResult = modelBuilder.BuildTrainingPipeline();

            // 4. evaluate quality of the pipeline
            var evaluateResult = modelBuilder.Evaluate();
            logger.LogInformation(evaluateResult.ToString());

            // 5. train the model
            var trainModelResult = modelBuilder.TrainModel();

            return evaluateResult;
        };

        public Func<IModelDefinitionBuilder<TInput, TResult>, ILogger, CancellationToken, Task>? ClassifyTestConfigurator { get; set; }

        public Func<IOptionsFactory<SourceLoaderOptions<TInput>>, string, SourceLoaderOptions<TInput>> SourceLoaderOptionsConfigurator { get; set; }
         = (sourceLoaderOptionsFactory, modelName) => sourceLoaderOptionsFactory.Create(modelName);

        public Func<IOptionsFactory<ModelLoaderOptions>, string, ModelLoaderOptions> ModelLoaderOptionsConfigurator { get; set; }
         = (modelLoaderOptionsFactory, modelName) => modelLoaderOptionsFactory.Create(modelName);
    }
}
