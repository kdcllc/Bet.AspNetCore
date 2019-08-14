using Microsoft.ML.Data;

namespace Bet.AspNetCore.Sample.Models
{
    public class SentimentPrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool IsToxic { get; set; }

        // Question:Isn't this column redundant? What we tell customers to do here?
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
