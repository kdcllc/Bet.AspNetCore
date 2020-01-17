using System.Collections.Generic;
using System.IO;

using Bet.Extensions.ML.ModelCreation.Results;

namespace Bet.Extensions.ML.ModelCreation
{
    public interface IModelBuilder<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        void LoadAndBuildDataView();

        TrainingPipelineResult BuildTrainingPipeline();

        TResult Evaluate();

        TrainModelResult TrainModel();

        Stream GetModelStream();

        void LoadData(IEnumerable<TInput> records);
    }
}
