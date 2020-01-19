using System.Collections.Generic;
using System.IO;

using Bet.Extensions.ML.ModelCreation.Results;

using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    public interface IModelDefinitionBuilder<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        ITransformer? Model { get; }

        MLContext MLContext { get; }

        void BuildDataView();

        TrainingPipelineResult BuildTrainingPipeline();

        TResult Evaluate();

        TrainModelResult TrainModel();

        Stream GetModelStream();

        void LoadData(IEnumerable<TInput> records);
    }
}
