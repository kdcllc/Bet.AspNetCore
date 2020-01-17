using Bet.Extensions.ML.ModelBuilder;

namespace Bet.Extensions.ML.Sentiment
{
    public class SentimentModelBuilderServiceOptions : ModelBuilderServiceOptions
    {
        public SentimentModelBuilderServiceOptions()
        {
            ModelName = nameof(SentimentModelCreationBuilder);
            ModelResultFileName = $"{ModelName}.json";
            ModelFileName = $"{ModelName}.zip";
        }
    }
}
