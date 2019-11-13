using Microsoft.ML.Data;

namespace Bet.Extensions.ML.Sentiment.Models
{
    public class SentimentIssue
    {
        public bool Label { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
