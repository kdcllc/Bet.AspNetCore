using System;
using System.Collections.Generic;

using Bet.Extensions.ML.Helpers;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders.Embedded
{
    public class EmbeddedSourceLoader<TInput> : SourceLoader<TInput> where TInput : class
    {
        public override Func<SourceLoaderFile<TInput>, List<TInput>> ProcessFile { get; set; }
            = (source) => EmbeddedResourceHelper.GetRecords<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
     }
}
