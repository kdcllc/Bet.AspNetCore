using Microsoft.Azure.Cosmos.Table;

namespace Bet.Extensions.UnitTest.ML
{
    public class SpamEntity : TableEntity
    {
        public SpamEntity()
        {
        }

        public SpamEntity(string partitionKey, string rowKey, string label, string message)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Label = label;
            Message = message;
        }

        public string Label { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
