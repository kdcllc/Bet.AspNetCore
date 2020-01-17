using System.Collections.Generic;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    public class EmbeddedSourceLoaderOptions<TInput> where TInput : class
    {
        public List<EmbeddedSources<TInput>> EmbeddedSourcesList { get; set; } = new List<EmbeddedSources<TInput>>();
    }
}
