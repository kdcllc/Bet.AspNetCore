using System;

using Bet.Extensions.ML.ModelCreation.Results;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelBuilderOptions<TResult> where TResult : MetricsResult
    {
        public double TestSlipFraction { get; set; } = 0.1;

        public Func<MLContext, TrainingPipelineResult>? TrainingPipelineConfigurator { get; set; }

        public Func<MLContext, ITransformer, string, IDataView, IEstimator<ITransformer>, TResult>? EvaluateConfigurator { get; set; }

        public Func<IDataView, IEstimator<ITransformer>, TrainModelResult> TrainModelConfigurator { get; set; } = (dataView, trainingPipeLine) =>
        {
            var model = trainingPipeLine.Fit(dataView);
            if (model == null)
            {
                throw new ArgumentNullException(nameof(dataView), "Training Model is Null");
            }

            return new TrainModelResult(model);
        };
    }
}
