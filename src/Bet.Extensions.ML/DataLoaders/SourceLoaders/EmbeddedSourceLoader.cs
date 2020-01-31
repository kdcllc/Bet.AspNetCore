using System;
using System.Collections.Generic;

using Bet.Extensions.ML.Helpers;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    public class EmbeddedSourceLoader<TInput> : SourceLoader<TInput> where TInput : class
    {
        public override Func<SourceLoaderFile<TInput>, IServiceProvider, string, List<TInput>> ProcessFile { get; set; }
            = (source, sp, modelName) => EmbeddedResourceHelper.GetRecords<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
     }
}
