using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelStorageProviders;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelEngineOptions<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        public string ModelName { get; set; } = string.Empty;

        public Func<ValueStopwatch, IModelBuilder<TInput, TResult>, IModelStorageProvider, ModelStorageProviderOptions, ILogger, CancellationToken, Task> TrainModelConfigurator { get; set; }
            = async (sw, modelBuilder, storageProvider, storageOptions, logger, cancellationToken) =>
        {
            // 1. load default ML data set
            logger.LogInformation("[LoadDataset][Started]");
            var data = await storageProvider.LoadRawDataAsync<TInput>(storageOptions.ModelSourceFileName, cancellationToken);

            modelBuilder.LoadData(data);

            modelBuilder.LoadAndBuildDataView();

            modelBuilder.BuildTrainingPipeline();

            if (modelBuilder?.DataView == null)
            {
                throw new NullReferenceException("DataView wasn't loaded");
            }

            logger.LogInformation(
                "[LoadDataset][Count]: {rowsCount} - elapsed time: {elapsed}ms",
                modelBuilder.DataView.GetRowCount(),
                sw.GetElapsedTime().TotalMilliseconds);

            // 2. build training pipeline
            logger.LogInformation("[BuildTrainingPipeline][Started]");
            var buildTrainingPipelineResult = modelBuilder.BuildTrainingPipeline();
            logger.LogInformation("[BuildTrainingPipeline][Ended] elapsed time: {elapsed}ms", buildTrainingPipelineResult.ElapsedMilliseconds);

            // 3. evaluate quality of the pipeline
            logger.LogInformation("[Evaluate][Started]");
            var evaluateResult = modelBuilder.Evaluate();
            logger.LogInformation("[Evaluate][Ended] elapsed time: {elapsed}ms", evaluateResult.ElapsedMilliseconds);
            logger.LogInformation(evaluateResult.ToString());
            await storageProvider.SaveModelResultAsync(evaluateResult, storageOptions.ModelResultFileName, cancellationToken);

            // 4. train the model
            logger.LogInformation("[TrainModel][Started]");
            var trainModelResult = modelBuilder.TrainModel();
            logger.LogInformation("[TrainModel][Ended] elapsed time: {elapsed}ms", trainModelResult.ElapsedMilliseconds);
            logger.LogInformation("[TrainModelAsync][Ended] elapsed time: {elapsed}ms", sw.GetElapsedTime().TotalMilliseconds);

            await Task.CompletedTask;
        };

        public Func<IModelBuilder<TInput, TResult>, ILogger, CancellationToken, Task>? ClassifyTestConfigurator { get; set; }
    }
}
