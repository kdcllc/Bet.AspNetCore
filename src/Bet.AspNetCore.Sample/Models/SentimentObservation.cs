using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;

namespace Bet.AspNetCore.Sample.Models
{
    public class SentimentObservation
    {
        [Required]
        [ColumnName("Text")]
        public string SentimentText { get; set; }
    }
}
