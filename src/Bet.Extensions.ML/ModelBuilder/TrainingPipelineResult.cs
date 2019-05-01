using Microsoft.ML;

namespace Bet.Extensions.ML.ModelBuilder
{
    public class TrainingPipelineResult
    {
        private readonly string _trainerName;

        public TrainingPipelineResult(IEstimator<ITransformer> trainingPipeLine, long elapsedMilliseconds, string trainerName)
        {
            TrainingPipeLine = trainingPipeLine;
            ElapsedMilliseconds = elapsedMilliseconds;
            _trainerName = trainerName;
        }

        public IEstimator<ITransformer> TrainingPipeLine { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public string TrainerName { get; set; }
    }
}
