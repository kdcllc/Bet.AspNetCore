using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class MLContextOptions
    {
        public MLContext MLContext { get; set; } = new MLContext();
    }
}
