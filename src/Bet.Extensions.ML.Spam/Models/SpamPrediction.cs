using Microsoft.ML.Data;

namespace Bet.Extensions.ML.Spam.Models
{
    public class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public string IsSpam { get; set; } = string.Empty;
    }
}
