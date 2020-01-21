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
        string ModelName { get; }

        ITransformer? Model { get; }

        MLContext MLContext { get; }

        void LoadData(IEnumerable<TInput> records);

        void BuildDataView();

        TrainingPipelineResult BuildTrainingPipeline();

        TResult Evaluate();

        TrainModelResult TrainModel();

        Stream GetModelStream();
    }
}
