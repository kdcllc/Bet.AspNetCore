using Microsoft.ML.Data;

namespace Bet.Extensions.ML.Spam.Models
{
    public class SpamInput
    {
        [LoadColumn(0)]
        public string Label { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string Message { get; set; } = string.Empty;
    }
}
