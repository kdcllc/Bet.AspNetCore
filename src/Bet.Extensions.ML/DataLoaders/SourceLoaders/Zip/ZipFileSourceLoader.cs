using System;
using System.Collections.Generic;

using Bet.Extensions.ML.Helpers;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders.Zip
{
    public class ZipFileSourceLoader<TInput> : SourceLoader<TInput> where TInput : class
    {
        public override Func<SourceLoaderFile<TInput>, List<TInput>> ProcessFile { get; set; }
            = (source) => FileHelper.GetRecordsFromZipFile<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
    }
}
