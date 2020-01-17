using Bet.Extensions.ML.ModelBuilder;

namespace Bet.Extensions.ML.Spam
{
    public class SpamModelBuilderServiceOptions : ModelBuilderServiceOptions
    {
        public SpamModelBuilderServiceOptions()
        {
            ModelName = nameof(SpamModelCreationBuilder);
            ModelResultFileName = $"{ModelName}.json";
            ModelFileName = $"{ModelName}.zip";
        }
    }
}
