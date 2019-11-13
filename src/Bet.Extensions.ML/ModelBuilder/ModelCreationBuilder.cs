using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.ML;

namespace Bet.Extensions.ML.ModelBuilder
{
    /// <inheritdoc/>
    public abstract class ModelCreationBuilder<TInput, TOutput, TResult> : IModelCreationBuilder<TInput, TOutput, TResult>
     where TInput : class
     where TOutput : class, new()
     where TResult : MetricsResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCreationBuilder{TInput, TOutput, TResult}"/> class.
        /// </summary>
        /// <param name="mLContext"></param>
        public ModelCreationBuilder(MLContext mLContext)
        {
            MLContext = mLContext ?? throw new ArgumentNullException(nameof(mLContext));
        }

        /// <inheritdoc/>
        public virtual IDataView? TrainingDataView { get; private set; } = default;

        /// <inheritdoc/>
        public virtual IDataView? TestDataView { get; private set; } = default;

        /// <inheritdoc/>
        public virtual IEstimator<ITransformer>? TrainingPipeLine { get; private set; } = default;

        /// <inheritdoc/>
        public virtual string TrainerName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public virtual IDataView? DataView { get; private set; } = default;

        /// <inheritdoc/>
        public virtual List<TInput> Records { get; set; } = new List<TInput>();

        /// <inheritdoc/>
        public virtual ITransformer? Model { get; private set; } = default;

        /// <inheritdoc/>
        public virtual MLContext MLContext { get; set; }

        /// <inheritdoc/>
        public virtual DataViewSchema? TrainingSchema { get; set; } = default;

        public abstract TrainingPipelineResult BuildTrainingPipeline();

        /// <inheritdoc/>
        public virtual TrainingPipelineResult BuildTrainingPipeline(Func<TrainingPipelineResult> builder)
        {
            var sw = ValueStopwatch.StartNew();

            var result = builder();

            TrainingPipeLine = result.TrainingPipeLine;
            TrainerName = result.TrainerName;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;
            return result;
        }

        /// <inheritdoc/>
        public virtual IModelCreationBuilder<TInput, TOutput, TResult> BuiltDataView(double testFraction = 0.1)
        {
            if (Records.Count > 0)
            {
                DataView = MLContext.Data.LoadFromEnumerable(Records);
                TrainingSchema = DataView.Schema;

                var trainTestSplit = MLContext.Data.TrainTestSplit(DataView, testFraction);
                TrainingDataView = trainTestSplit.TrainSet;
                TestDataView = trainTestSplit.TestSet;
            }
            else
            {
                throw new ArgumentException($"{nameof(DataView)} doesn't have any records.");
            }

            return this;
        }

        /// <inheritdoc/>
        public abstract TResult Evaluate();

        /// <inheritdoc/>
        public virtual TResult Evaluate(Func<IDataView?, IEstimator<ITransformer>?, TResult> builder)
        {
            var sw = ValueStopwatch.StartNew();

            var result = builder(TestDataView, TrainingPipeLine);

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;

            return result;
        }

        /// <inheritdoc/>
        public virtual IModelCreationBuilder<TInput, TOutput, TResult> LoadData(IEnumerable<TInput> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Records.AddRange(data);

            return this;
        }

        /// <inheritdoc/>
        public abstract IModelCreationBuilder<TInput, TOutput, TResult> LoadDefaultData();

        public virtual void SaveModel(Action<MLContext, ITransformer?, string, DataViewSchema?> builder, string modelRelativePath)
        {
            builder(MLContext, Model, modelRelativePath, TrainingSchema);
        }

        /// <inheritdoc/>
        public virtual void SaveModel(string modelRelativePath)
        {
            SaveModel(
                (mlContext, mlModel, path, modelInputSchema) =>
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        mlContext.Model.Save(mlModel, modelInputSchema, fs);
                    }
                }, modelRelativePath);
        }

        public virtual MemoryStream GetModelStream()
        {
            var stream = new MemoryStream();

            MLContext.Model.Save(Model, TrainingSchema, stream);
            return stream;
        }

        /// <inheritdoc/>
        public virtual TrainModelResult TrainModel()
        {
            return TrainModel((dataView) =>
            {
                var model = TrainingPipeLine?.Fit(dataView);
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(dataView), "Training Model is Null");
                }

                return new TrainModelResult(model);
            });
        }

        /// <inheritdoc/>
        public virtual TrainModelResult TrainModel(Func<IDataView?, TrainModelResult> builder)
        {
            var sw = ValueStopwatch.StartNew();

            var result = builder(TrainingDataView);
            Model = result.Model;

            result.ElapsedMilliseconds = (long)sw.GetElapsedTime().TotalMilliseconds;
            return result;
        }
    }
}
