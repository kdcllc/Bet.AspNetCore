using System;
using System.Collections.Generic;
using System.Threading;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.Helpers;

using CsvHelper.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.ML.Azure.SourceLoaders
{
    public class AzureStorageSourceLoader<TInput> : SourceLoader<TInput> where TInput : class
    {
        public override Func<SourceLoaderFile<TInput>, IServiceProvider, string, List<TInput>> ProcessFile { get; set; }
        = (options, sp, modelName) =>
        {
            var config = new Configuration
            {
                Delimiter = options.Delimiter,
                HasHeaderRecord = options.HasHeaderRecord
            };

            var storage = sp.GetRequiredService<IStorageBlob<StorageBlobOptions>>();

            var download = storage.GetAsync(modelName, options.FileName, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            if (download == null)
            {
                return new List<TInput>();
            }

            if (options.FileName.Contains(".zip"))
            {
                return FileHelper.GetRecordsFromZipFile<TInput>(download, config);
            }

            return FileHelper.GetRecords<TInput>(download, config);
        };
    }
}
