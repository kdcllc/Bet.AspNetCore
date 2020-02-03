using System;
using System.Collections.Generic;

using Bet.Extensions.ML.Helpers;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    public class FileSourceLoader<TInput> : SourceLoader<TInput> where TInput : class
    {
        public override Func<SourceLoaderFile<TInput>, IServiceProvider, string, List<TInput>> ProcessFile { get; set; }
        = (source, sp, modelName) =>
        {
            if (source.FileName.Contains(".zip"))
            {
                return FileHelper.GetRecordsFromZipFile<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
            }

            return FileHelper.GetRecords<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
        };
    }
}
