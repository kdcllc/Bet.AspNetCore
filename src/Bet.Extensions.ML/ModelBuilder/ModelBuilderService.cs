using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelStorageProviders;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.ModelBuilder
{
    /// <summary>
    /// Override this class to get the functionality for <see cref="IModelBuilderService"/>.
    /// </summary>
    /// <typeparam name="TInput">The type of the input ML.NET model.</typeparam>
    /// <typeparam name="TOutput">The output of the ML.NET model that is used for prediction.</typeparam>
    /// <typeparam name="TResult">The ML.NET result of the prediction.</typeparam>
    public abstract class ModelBuilderService<TInput, TOutput, TResult> : IModelBuilderService
        where TInput : class
        where TOutput : class, new()
        where TResult : MetricsResult
    {
        private readonly IModelCreationBuilder<TInput, TOutput, TResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly ILogger _logger;

        public ModelBuilderService(
            IModelCreationBuilder<TInput, TOutput, TResult> modelBuilder,
            IModelStorageProvider storageProvider,
            ILogger logger)
        {
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract string Name { get; set; }

        public abstract Task ClassifyTestAsync(CancellationToken cancellationToken);

        public virtual async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            _logger.LogInformation("[SaveModelAsync][Started]");
            var sw = ValueStopwatch.StartNew();

            var readStream = _modelBuilder.GetModelStream();
            await _storageProvider.SaveModelAsync(Name, readStream, cancellationToken);

            _logger.LogInformation("[SaveModelAsync][Ended] elapsed time: {elapsed} ms", sw.GetElapsedTime().TotalMilliseconds);
        }

        /// <summary>
        /// The following steps are executed in the pipeline:
        /// 1. LoadDefaultData().BuiltDataView()
        /// 2. BuildTrainingPipeline()
        /// 3. Evaluate()
        /// 4. SaveModelResultAsync()
        /// 5. TrainModel().
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task TrainModelAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TrainModelAsync][Started]");
            var sw = ValueStopwatch.StartNew();

            // 1. load default ML data set
            _logger.LogInformation("[LoadDataset][Started]");
            _modelBuilder.LoadDefaultData().BuiltDataView();
            _logger.LogInformation(
                "[LoadDataset][Count]: {rowsCount} - elapsed time: {elapsed} ms",
                _modelBuilder.DataView.GetRowCount(),
                sw.GetElapsedTime().TotalMilliseconds);

            // 2. build training pipeline
            _logger.LogInformation("[BuildTrainingPipeline][Started]");
            var buildTrainingPipelineResult = _modelBuilder.BuildTrainingPipeline();
            _logger.LogInformation("[BuildTrainingPipeline][Ended] elapsed time: {elapsed} ms", buildTrainingPipelineResult.ElapsedMilliseconds);

            // 3. evaluate quality of the pipeline
            _logger.LogInformation("[Evaluate][Started]");
            var evaluateResult = _modelBuilder.Evaluate();
            _logger.LogInformation("[Evaluate][Ended] elapsed time: {elapsed} ms", evaluateResult.ElapsedMilliseconds);
            _logger.LogInformation(evaluateResult.ToString());
            await _storageProvider.SaveModelResultAsync(evaluateResult, Name, cancellationToken);

            // 4. train the model
            _logger.LogInformation("[TrainModel][Started]");
            var trainModelResult = _modelBuilder.TrainModel();
            _logger.LogInformation("[TrainModel][Ended] elapsed time: {elapsed} ms", trainModelResult.ElapsedMilliseconds);
            _logger.LogInformation("[TrainModelAsync][Ended] elapsed time: {elapsed} ms", sw.GetElapsedTime().TotalMilliseconds);

            await Task.CompletedTask;
        }
    }
}
