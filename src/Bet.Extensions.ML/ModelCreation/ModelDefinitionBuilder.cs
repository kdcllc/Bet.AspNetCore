using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Bet.Extensions.ML.ModelCreation.Results;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
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

        public ModelDefinitionBuilder(
            MLContext mLContext,
            ModelDefinitionBuilderOptions<TResult> options,
            ILogger<ModelDefinitionBuilder<TInput, TResult>> logger)
        {
            MLContext = mLContext ?? throw new ArgumentNullException(nameof(mLContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = options;
        }

        public string ModelName => _options.ModelName;

        public MLContext MLContext { get; }

        public ITransformer? Model { get; private set; }

        public virtual void LoadData(IEnumerable<TInput> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[{methodName}][Started]:", nameof(LoadData));

            _rawDataSet.AddRange(records);

            _logger.LogInformation(
                "[{methodName}][Ended]: Record count: {count} - elapsed time: {elapsed}ms",
                nameof(LoadData),
                _rawDataSet.Count,
                sw.GetElapsedTime().Milliseconds);
        }

        public void BuildDataView()
        {
            // 1. check if data exists
            if (_rawDataSet.Count == 0)
            {
                throw new ArgumentException("No records were loaded for the model.");
            }

            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[{methodName}][Started]:", nameof(BuildDataView));

            // 1. create dataview
            _dataView = MLContext.Data.LoadFromEnumerable(_rawDataSet);

            if (_dataView == null)
            {
                throw new NullReferenceException($"{nameof(_dataView)} failed to load data.");
            }

            // schema is used to save the file
            _trainingSchema = _dataView.Schema;

            // 3. split the dataset based on proportions
            var trainTestSplit = MLContext.Data.TrainTestSplit(_dataView, _options.TestSlipFraction);
            _trainingDataView = trainTestSplit.TrainSet;
            _testDataView = trainTestSplit.TestSet;

            _logger.LogInformation(
                "[{methodName}][Ended]: Rows Count: {rowsCount} - elapsed time: {elapsed}ms",
                nameof(BuildDataView),
                _dataView.GetRowCount(),
                sw.GetElapsedTime().TotalMilliseconds);
        }

        public TrainingPipelineResult BuildTrainingPipeline()
        {
            if (_options.TrainingPipelineConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.TrainingPipelineConfigurator)} wasn't configured.");
            }

            _logger.LogInformation("[{methodName}][Started]", nameof(BuildTrainingPipeline));

            var sw = ValueStopwatch.StartNew();

            var result = _options.TrainingPipelineConfigurator(MLContext);

            _trainingPipeLine = result.TrainingPipeLine;
            _trainerName = result.TrainerName;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;

            _logger.LogInformation("[{methodName}][Ended] elapsed time: {elapsed}ms", nameof(BuildTrainingPipeline), result.ElapsedMilliseconds);

            return result;
        }

        public virtual TResult Evaluate()
        {
            if (_options.EvaluateConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.EvaluateConfigurator)} wasn't configured.");
            }

            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[{methodName}][Started]", nameof(Evaluate));

            if (_testDataView == null
                || _trainingPipeLine == null)
            {
                throw new ArgumentNullException($"{nameof(_testDataView)} or {nameof(_trainingPipeLine)} are null.");
            }

            var result = _options.EvaluateConfigurator(MLContext, Model!, _trainerName, _testDataView, _trainingPipeLine);

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;

            _logger.LogInformation("[{methodName}][Ended] elapsed time: {elapsed}ms", nameof(Evaluate), result.ElapsedMilliseconds);

            return result;
        }

        public TrainModelResult TrainModel()
        {
            if (_options.TrainModelConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelDefinitionBuilderOptions<TResult>.TrainModelConfigurator)} wasn't configured.");
            }

            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[{methodName}][Started]", nameof(TrainModel));

            if (_trainingDataView == null || _trainingPipeLine == null)
            {
                throw new ArgumentNullException($"{nameof(_trainingDataView)} or {_trainingPipeLine} are null.");
            }

            var result = _options.TrainModelConfigurator(_trainingDataView, _trainingPipeLine);
            Model = result.Model;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;

            _logger.LogInformation("[{methodName}][Ended] elapsed time: {elapsed}ms", nameof(TrainModel), result.ElapsedMilliseconds);

            return result;
        }

        public Stream GetModelStream()
        {
            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[{methodName}][Started]", nameof(GetModelStream));

            var stream = new MemoryStream();

            MLContext.Model.Save(Model, _trainingSchema, stream);

            _logger.LogInformation("[{methodName}][Ended] elapsed time: {elapsed}ms", nameof(GetModelStream), sw.GetElapsedTime().TotalMilliseconds);

            return stream;
        }
    }
}
