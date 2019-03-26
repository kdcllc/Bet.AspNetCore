using Microsoft.ML.Data;

namespace Bet.Hosting.Sample.ML.Models
{
    internal class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsSpam { get; set; }

        public float Score { get; set; }
        public float Probability { get; set; }
    }
}
