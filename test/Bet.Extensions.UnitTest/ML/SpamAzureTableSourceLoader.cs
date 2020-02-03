using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.UnitTest.ML
{
    public class SpamAzureTableSourceLoader : SourceLoader<SpamInput>
    {
        public override Func<SourceLoaderFile<SpamInput>, IServiceProvider, string, List<SpamInput>> ProcessFile { get; set; }

        public override IEnumerable<SpamInput> LoadData()
        {
            var list = new List<SpamInput>();

            var table = Serviceprovider.GetRequiredService<IStorageTable<StorageTableOptions>>();

            var query = new TableQuery<SpamEntity>();

            var queryResults = table.QueryAsync<SpamEntity>(ModelName, query, CancellationToken.None)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            var results = queryResults.Select(row =>
            {
                return new SpamInput
                {
                    Label = row.Label,
                    Message = row.Message
                };
            });

            list.AddRange(results);

            return list;
        }
    }
}
