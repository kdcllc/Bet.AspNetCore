using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation.Results
{
    public class TrainModelResult
    {
        public TrainModelResult(ITransformer model, long elapsedMilliseconds = default)
        {
            Model = model;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public ITransformer Model { get; set; }

        public long ElapsedMilliseconds { get; set; }
    }
}
