using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Bet.Extensions.UnitTest.AzureStorage
{
    public class StorageBlobTests
    {
        [RunnableInDebugOnly]
        public async Task Test()
        {
            var settings = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://bet.vault.azure.net/" },
                { "AzureStorage:DefaultAccount:Name", "betstorage" },
            };

            var services = new ServiceCollection();
            var configurations = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(configurations);

            services.AddAzureStorageAccount().AddAzureBlobContainer("models", "models");

            var sp = services.BuildServiceProvider();

            var storage = sp.GetRequiredService<IStorageBlob<StorageBlobOptions>>();

            var fileName = "SpamModel1.zip";

            // var fileName = "test.json";
            // await storage.AddAsync("models", new { content = "This is a test to see if this works" }, fileName);
            var blob = await storage.GetBlobAsync("models", fileName, CancellationToken.None);
            await blob.FetchAttributesAsync();
            Console.WriteLine(blob.Properties.ETag);

            var stream = await storage.GetAsync("models", fileName, CancellationToken.None);
        }
    }
}
