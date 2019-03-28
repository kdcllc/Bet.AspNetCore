using Microsoft.ML.Data;

namespace Bet.AspNetCore.Sample.Models
{
    public class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsSpam { get; set; }

        public float Score { get; set; }
        public float Probability { get; set; }
    }
}
