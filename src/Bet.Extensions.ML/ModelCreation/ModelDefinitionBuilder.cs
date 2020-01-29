using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Bet.Extensions.ML.ModelCreation.Results;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    /// <inheritdoc/>
    public class ModelDefinitionBuilder<TInput, TResult> : IModelDefinitionBuilder<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        private readonly ModelDefinitionBuilderOptions<TResult> _options;
        private readonly ILogger<ModelDefinitionBuilder<TInput, TResult>> _logger;

        private List<TInput> _rawDataSet = new List<TInput>();
        private IDataView? _dataView;
        private DataViewSchema? _trainingSchema;
        private IDataView? _trainingDataView;
        private IDataView? _testDataView;
        private IEstimator<ITransformer>? _trainingPipeLine;
        private string _trainerName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDefinitionBuilder{TInput, TResult}"/> class.
        /// </summary>
        /// <param name="mLContext"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ModelDefinitionBuilder(
            IOptions<MLContextOptions> mLContext,
            ModelDefinitionBuilderOptions<TResult> options,
            ILogger<ModelDefinitionBuilder<TInput, TResult>> logger)
        {
            MLContext = mLContext.Value.MLContext ?? throw new ArgumentNullException(nameof(mLContext));

            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public string ModelName => _options.ModelName;

        /// <inheritdoc/>
        public MLContext MLContext { get; }

        /// <inheritdoc/>
        public ITransformer? Model { get; private set; }

        /// <inheritdoc/>
        public virtual void LoadData(IEnumerable<TInput> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var sw = ValueStopwatch.StartNew();

            Log.StartProcess(_logger, nameof(LoadData), ModelName);

            _rawDataSet.AddRange(records);

            Log.EndProcess(_logger, nameof(LoadData), ModelName, _rawDataSet.Count, sw.GetElapsedTime());
        }

        /// <inheritdoc/>
        public void BuildDataView()
        {
            // 1. check if data exists
            if (_rawDataSet.Count == 0)
            {
                throw new ArgumentException($"No records were loaded for the model: {ModelName}.");
            }

            var sw = ValueStopwatch.StartNew();

            Log.StartProcess(_logger, nameof(BuildDataView), ModelName);

            // 1. create dataview
            _dataView = MLContext.Data.LoadFromEnumerable(_rawDataSet);

            if (_dataView == null)
            {
                throw new NullReferenceException($"{nameof(_dataView)} failed to load data for model: {ModelName}.");
            }

            // schema is used to save the file
            _trainingSchema = _dataView.Schema;

            // 3. split the dataset based on proportions
            var trainTestSplit = MLContext.Data.TrainTestSplit(_dataView, _options.TestSlipFraction);
            _trainingDataView = trainTestSplit.TrainSet;
            _testDataView = trainTestSplit.TestSet;

            Log.EndProcess(_logger, nameof(BuildDataView), ModelName, _dataView.GetRowCount() ?? 0, sw.GetElapsedTime());
        }

        /// <inheritdoc/>
        public TrainingPipelineResult BuildTrainingPipeline()
        {
            if (_options.TrainingPipelineConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.TrainingPipelineConfigurator)} wasn't configured for model:{ModelName}.");
            }

            Log.StartProcess(_logger, nameof(BuildTrainingPipeline), ModelName);

            var sw = ValueStopwatch.StartNew();

            var result = _options.TrainingPipelineConfigurator(MLContext);

            _trainingPipeLine = result.TrainingPipeLine;
            _trainerName = result.TrainerName;

            var elapsedTime = sw.GetElapsedTime();
            result.ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds;

            Log.EndProcess(_logger, nameof(BuildTrainingPipeline), ModelName, elapsedTime);

            return result;
        }

        /// <inheritdoc/>
        public virtual TResult Evaluate()
        {
            if (_options.EvaluateConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.EvaluateConfigurator)} wasn't configured for model:{ModelName}.");
            }

            var sw = ValueStopwatch.StartNew();

            Log.StartProcess(_logger, nameof(Evaluate), ModelName);

            if (_testDataView == null
                || _trainingPipeLine == null)
            {
                throw new ArgumentNullException($"{nameof(_testDataView)} or {nameof(_trainingPipeLine)} are null for model:{ModelName}.");
            }

            var result = _options.EvaluateConfigurator(MLContext, Model!, _trainerName, _testDataView, _trainingPipeLine);

            var elapsedTime = sw.GetElapsedTime();
            result.ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds;

            Log.EndProcess(_logger, nameof(Evaluate), ModelName, elapsedTime);

            return result;
        }

        /// <inheritdoc/>
        public TrainModelResult TrainModel()
        {
            if (_options.TrainModelConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.TrainModelConfigurator)} wasn't configured for model:{ModelName}.");
            }

            var sw = ValueStopwatch.StartNew();

            Log.StartProcess(_logger, nameof(TrainModel), ModelName);

            if (_trainingDataView == null || _trainingPipeLine == null)
            {
                throw new ArgumentNullException($"{nameof(_trainingDataView)} or {_trainingPipeLine} are null for model:{ModelName}.");
            }

            var result = _options.TrainModelConfigurator(_trainingDataView, _trainingPipeLine);
            Model = result.Model;

            var elapsedTime = sw.GetElapsedTime();
            result.ElapsedMilliseconds = (long)elapsedTime.TotalMilliseconds;

            Log.EndProcess(_logger, nameof(TrainModel), ModelName, elapsedTime);

            return result;
        }

        /// <inheritdoc/>
        public Stream GetModelStream()
        {
            var sw = ValueStopwatch.StartNew();

            Log.StartProcess(_logger, nameof(GetModelStream), ModelName);

            var stream = new MemoryStream();

            MLContext.Model.Save(Model, _trainingSchema, stream);

            Log.EndProcess(_logger, nameof(GetModelStream), ModelName, sw.GetElapsedTime());

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

#nullable disable
#pragma warning disable SA1201 // Elements should appear in the correct order
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId StartProcess = new EventId(100, nameof(StartProcess));
                public static readonly EventId EndProcess = new EventId(101, nameof(EndProcess));
                public static readonly EventId EndWithRecordCountProcess = new EventId(102, nameof(EndWithRecordCountProcess));
            }

            private static readonly Action<ILogger, string, string, Exception> _startProcess = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                EventIds.StartProcess,
                "[{methodName}][Started] Model: {modelName};");

            private static readonly Action<ILogger, string, string, double, Exception> _endProcess = LoggerMessage.Define<string, string, double>(
                LogLevel.Debug,
                EventIds.EndProcess,
                "[{methodName}][Ended] Model: {modelName}; elapsed time: {elapsed}ms");

            private static readonly Action<ILogger, string, string, long, double, Exception> _endWithRecordCountProcess = LoggerMessage.Define<string, string, long, double>(
                LogLevel.Debug,
                EventIds.EndWithRecordCountProcess,
                "[{methodName}][Ended] Model: {modelName}; Record count: {count}; Elapsed time: {elapsed}ms");

            public static void StartProcess(ILogger logger, string methodName, string modelName)
            {
                _startProcess(logger, methodName, modelName, null);
            }

            public static void EndProcess(ILogger logger, string methodName, string modelName, TimeSpan time)
            {
                _endProcess(logger, methodName, modelName, time.TotalMilliseconds, null);
            }

            public static void EndProcess(ILogger logger, string methodName, string modelName, long count, TimeSpan time)
            {
                _endWithRecordCountProcess(logger, methodName, modelName, count, time.TotalMilliseconds, null);
            }
        }
#pragma warning restore SA1201 // Elements should appear in the correct order
#nullable restore
    }
}
