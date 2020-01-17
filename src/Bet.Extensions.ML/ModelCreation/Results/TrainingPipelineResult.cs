using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation.Results
{
    public class TrainingPipelineResult
    {
        public TrainingPipelineResult(
            IEstimator<ITransformer> trainingPipeLine,
            string trainerName,
            long elapsedMilliseconds = default)
        {
            TrainingPipeLine = trainingPipeLine;
            ElapsedMilliseconds = elapsedMilliseconds;
            TrainerName = trainerName;
        }

        public IEstimator<ITransformer> TrainingPipeLine { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public string TrainerName { get; set; }
    }
}
