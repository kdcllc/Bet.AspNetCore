using Microsoft.ML.Data;

namespace Bet.AspNetCore.Sample.Models
{
    internal class Message
    {
        //[LoadColumn(0)]
        public string TextLabel { get; set; }

        //[LoadColumn(1)]
        public string Text { get; set; }

        public bool Label { get; set; }
    }
}
