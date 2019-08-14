using System.ComponentModel.DataAnnotations;
using Microsoft.ML.Data;

namespace Bet.AspNetCore.Sample.Models
{
    public class SentimentObservation
    {
        [Required]
        [ColumnName("Text")]
        public string SentimentText { get; set; }
    }
}
