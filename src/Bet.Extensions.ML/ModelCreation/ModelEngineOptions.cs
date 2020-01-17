using System;

using Bet.Extensions.ML.ModelCreation.Results;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelEngineOptions<TResult> where TResult : MetricsResult
    {
        public double TestSlipFraction { get; set; } = 0.1;

        public Func<MLContext, TrainingPipelineResult>? TrainingPipelineConfigurator { get; set; }

        public Func<IDataView, IEstimator<ITransformer>, TResult>? EvaluateConfigurator { get; set; }

        public Func<IDataView, TrainModelResult>? TrainModelConfigurator { get; set; }
    }
}
