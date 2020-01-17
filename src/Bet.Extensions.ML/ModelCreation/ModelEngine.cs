using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Bet.Extensions.ML.ModelCreation.DataLoaders;
using Bet.Extensions.ML.ModelCreation.Results;

using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelEngine<TInput, TOutput, TResult> : IModelEngine<TInput, TResult> where TInput : class
         where TOutput : class, new()
         where TResult : MetricsResult
    {
        private readonly ISourceLoader<TInput> _sourceLoader;
        private readonly ModelEngineOptions<TResult> _options;

        private MLContext _mLContext;
        private List<TInput> _rawDataSet = new List<TInput>();
        private IDataView? _dataView;
        private DataViewSchema? _trainingSchema;
        private IDataView? _trainingDataView;
        private IDataView? _testDataView;
        private IEstimator<ITransformer>? _trainingPipeLine;
        private ITransformer? _model;

        public ModelEngine(
            MLContext mLContext,
            ISourceLoader<TInput> sourceLoader,
            IOptions<ModelEngineOptions<TResult>> options)
        {
            _mLContext = mLContext ?? throw new ArgumentNullException(nameof(mLContext));
            _sourceLoader = sourceLoader ?? throw new ArgumentNullException(nameof(sourceLoader));

            _options = options.Value;
        }

        public virtual void LoadData(IEnumerable<TInput> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            _rawDataSet.AddRange(records);
        }

        public void LoadAndBuildDataView()
        {
            // 1. load the data
            var data = _sourceLoader.LoadData();
            _rawDataSet.AddRange(data);

            if (_rawDataSet.Count == 0)
            {
                throw new ArgumentException("No records were loaded for the model.");
            }

            // 2. create dataview
            _dataView = _mLContext.Data.LoadFromEnumerable(_rawDataSet);

            // schema is used to save the file
            _trainingSchema = _dataView.Schema;

            // 3. split the dataset based on proportions
            var trainTestSplit = _mLContext.Data.TrainTestSplit(_dataView, _options.TestSlipFraction);
            _trainingDataView = trainTestSplit.TrainSet;
            _testDataView = trainTestSplit.TestSet;
        }

        public TrainingPipelineResult BuildTrainingPipeline()
        {
            if (_options.TrainingPipelineConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelEngineOptions<TResult>.TrainingPipelineConfigurator)} wasn't configured.");
            }

            var sw = ValueStopwatch.StartNew();

            var result = _options.TrainingPipelineConfigurator(_mLContext);

            _trainingPipeLine = result.TrainingPipeLine;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;
            return result;
        }

        public virtual TResult Evaluate()
        {
            if (_options.EvaluateConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelEngineOptions<TResult>.EvaluateConfigurator)} wasn't configured.");
            }

            var sw = ValueStopwatch.StartNew();

            if (_testDataView == null
                || _trainingPipeLine == null)
            {
                throw new ArgumentNullException($"{nameof(_testDataView)} or {nameof(_trainingPipeLine)} are have null values.");
            }

            var result = _options.EvaluateConfigurator(_testDataView, _trainingPipeLine);

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;

            return result;
        }

        public TrainModelResult TrainModel()
        {
            if (_options.TrainModelConfigurator == null)
            {
                throw new ArgumentException($"{nameof(ModelEngineOptions<TResult>.TrainModelConfigurator)} wasn't configured.");
            }

            var sw = ValueStopwatch.StartNew();

            if (_trainingDataView == null)
            {
                throw new ArgumentNullException($"{nameof(_trainingDataView)} is null.");
            }

            var result = _options.TrainModelConfigurator(_trainingDataView);
            _model = result.Model;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;
            return result;
        }

        public Stream GetModelStream()
        {
            var stream = new MemoryStream();

            _mLContext.Model.Save(_model, _trainingSchema, stream);
            return stream;
        }
    }
}
