namespace Bet.Extensions.ML.Sentiment.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "None")]
    internal class InputSentimentIssueRow
    {
        public int Label { get; set; }

        public double rev_id { get; set; }

        public string comment { get; set; } = string.Empty;
    }
}
