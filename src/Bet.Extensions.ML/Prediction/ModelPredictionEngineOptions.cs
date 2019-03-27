using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngineOptions
    {
        public string ModelName { get; set; } = Constants.MLDefaultModelName;

        public Func<MLContext> MLContext { get; set; } = () => new MLContext();

        public int MaximumObjectsRetained { get; set; } = -1;

        public Func<MLContext, ITransformer> CreateModel { get; set; }

        public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    }
}
