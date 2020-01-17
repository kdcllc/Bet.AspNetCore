using System.Collections.Generic;
using System.IO;
using Bet.Extensions.ML.ModelCreation.Results;

namespace Bet.Extensions.ML.ModelCreation
{
    public interface IModelEngine<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        TrainingPipelineResult BuildTrainingPipeline();
        TResult Evaluate();
        Stream GetModelStream();
        void LoadAndBuildDataView();
        void LoadData(IEnumerable<TInput> records);
        TrainModelResult TrainModel();
    }
}