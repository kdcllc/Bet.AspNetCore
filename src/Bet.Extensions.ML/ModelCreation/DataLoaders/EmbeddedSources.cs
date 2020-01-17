using System;
using System.Collections.Generic;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    public class EmbeddedSources<TInput> where TInput : class
    {
        public string FileName { get; set; } = string.Empty;

        public string Delimiter { get; set; } = string.Empty;

        public bool HasHeaderRecord { get; set; }

        public Func<IEnumerable<TInput>>? Overrides { get; set; } = null;
    }
}
