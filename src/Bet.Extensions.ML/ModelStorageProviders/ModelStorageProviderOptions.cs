namespace Bet.Extensions.ML.ModelStorageProviders
{
    public class ModelStorageProviderOptions
    {
        /// <summary>
        /// The name of the model to be build.
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        public string ModelFileName { get; set; } = string.Empty;

        public string ModelResultFileName { get; set; } = string.Empty;

        public string ModelSourceFileName { get; set; } = string.Empty;
    }
}
