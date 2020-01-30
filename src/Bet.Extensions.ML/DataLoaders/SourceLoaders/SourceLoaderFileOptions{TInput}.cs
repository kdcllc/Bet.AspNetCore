using System.Collections.Generic;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    public class SourceLoaderFileOptions<TInput> where TInput : class
    {
        public List<SourceLoaderFile<TInput>> Sources { get; set; } = new List<SourceLoaderFile<TInput>>();
    }
}
