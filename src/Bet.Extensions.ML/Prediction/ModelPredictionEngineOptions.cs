using Microsoft.ML;
using System;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngineOptions
    {
        public string Name { get; set; } = Constants.MLDefaultModelName;

        public MLContext MLContext { get; set; } = new MLContext();

        public int MaximumObjectsRetained { get; set; } = -1;

        public Func<MLContext, ITransformer> CreateModel { get; set; }
    }
}
