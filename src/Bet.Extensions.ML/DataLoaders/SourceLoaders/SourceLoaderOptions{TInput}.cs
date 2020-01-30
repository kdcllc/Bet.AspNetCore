namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    public class SourceLoaderOptions<TInput> where TInput : class
    {
        public SourceLoader<TInput> SourceLoader { get; set; } = default!;
    }
}
