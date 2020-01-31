using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML.Azure.SourceLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.Helpers;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace Bet.Extensions.UnitTest.ML
{
    public class SourceLoaderTests
    {
        [Fact(Skip ="Integration")]
        public void AzureStorageSourceLoader_LoadData_Successfully()
        {
            var modelName = "spamModel";

            var settings = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://bet.vault.azure.net/" },
                { "AzureStorage:DefaultAccount:Name", "betstorage" },
            };

            var services = new ServiceCollection();

            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(config);

            services.AddAzureStorageAccount()
                    .AddAzureBlobContainer(modelName, "sources");

            services.AddSourceLoader<SpamInput, AzureStorageSourceLoader<SpamInput>>(modelName, options =>
            {
                options.Sources.Add(
                    new SourceLoaderFile<SpamInput>
                    {
                       FileName = "SMSSpamCollection.tsv",
                       Delimiter = "\t"
                    });

                options.Sources.Add(
                      new SourceLoaderFile<SpamInput>
                      {
                          FileName = "SpamModel-Source.zip",
                          Delimiter = ","
                      });
            });

            var sp = services.BuildServiceProvider();

            var factory = sp.GetRequiredService<IOptionsFactory<SourceLoaderOptions<SpamInput>>>();

            var loader = factory.Create(modelName);

            var records = loader.SourceLoader.LoadData();

            Assert.Equal(11144, records.Count());
        }

        [Fact(Skip = "Integration")]
        public void SpamAzureTableSourceLoader_LoadData_Successfully()
        {
            var modelName = "spamModel";
            var tableName = "spamdata";

            var settings = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://bet.vault.azure.net/" },
                { "AzureStorage:TableAccount:Name", "betstorage" },
            };

            var services = new ServiceCollection();

            var builderConfig = new ConfigurationBuilder().AddInMemoryCollection(settings);

            builderConfig.AddAzureKeyVault(hostingEnviromentName: "Development");
            var config = builderConfig.Build();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(config);

            services.AddAzureStorageAccount(sectionName: "TableAccount")
                    .AddAzureTable(modelName, tableName);

            services.AddSourceLoader<SpamInput, SpamAzureTableSourceLoader>(modelName, _ => { });

            var sp = services.BuildServiceProvider();

            var factory = sp.GetRequiredService<IOptionsFactory<SourceLoaderOptions<SpamInput>>>();

            var loader = factory.Create(modelName);

            var records = loader.SourceLoader.LoadData();

            Assert.NotNull(records);
        }

        [Fact(Skip ="Integration")]
        public async Task AzureTable_Successfull()
        {
            var modelName = "spamModel";
            var tableName = "spamdata";

            var settings = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://bet.vault.azure.net/" },
                { "AzureStorage:TableAccount:Name", "betstorage" },
            };

            var services = new ServiceCollection();
            var configurations = new ConfigurationBuilder()
                                        .AddInMemoryCollection(settings);

            configurations.AddAzureKeyVault(hostingEnviromentName: "Development");
            var config = configurations.Build();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(config);

            services.AddAzureStorageAccount(sectionName: "TableAccount").AddAzureTable(modelName, tableName);

            services.AddSourceLoader<SpamInput, FileSourceLoader<SpamInput>>(modelName, options =>
            {
                options.Sources = new List<SourceLoaderFile<SpamInput>>
                {
                    new SourceLoaderFile<SpamInput>
                    {
                        FileName = "ML/DataSets/SMSSpamCollection",
                        Delimiter = "\t",
                        HasHeaderRecord = false
                    }
                };
            });

            var sp = services.BuildServiceProvider();

            var table = sp.GetRequiredService<IStorageTable<StorageTableOptions>>();

            var factory = sp.GetRequiredService<IOptionsFactory<SourceLoaderOptions<SpamInput>>>();

            var loader = factory.Create(modelName);

            var records = loader.SourceLoader.LoadData();

            var partitionKey = "f89528e8";

            var ent = records.Select(r => new SpamEntity(partitionKey, Guid.NewGuid().ToString(), r.Label, r.Message))
                .Where(x => x != null);

            var deleteQuery = new TableQuery<TableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var projectionQuery = new TableQuery()
                                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                                        .Select(new[] { "RowKey" });

            var deleteResults = await table.DeleteBatchAsync(modelName, deleteQuery, 100);

            var results = await table.BatchInsertOrUpdateAsync<SpamEntity>(modelName, ent, 100);

            Assert.NotNull(results);
        }

        [Fact]
        public void FileHelper_GetRecords_Successfully()
        {
            // read the records from the file
            var records = FileHelper.GetRecords<SpamInput>("ML/DataSets/SMSSpamCollection", "\t", false);
            Assert.Equal(5572, records.Count);

            // write records to zip file
            var zipFileBytes = FileHelper.GetZipFileFromRecords(records, ",", false);
            var zipFileName = "SpamModel.zip";
            FileHelper.SaveFile(zipFileBytes, zipFileName);

            var zipRecords = FileHelper.GetRecordsFromZipFile<SpamInput>(zipFileName, ",", false);
            Assert.Equal(5572, zipRecords.Count);
        }

        [Fact]
        public void GetRecordsFromZipFile_Successfully()
        {
            var fileName = "ML/DataSets/SpamModel-Combine.zip";

            var records = FileHelper.GetRecordsFromZipFile<SpamInput>(fileName, ",", false);

            Assert.Equal(16716, records.Count);
        }

        [Theory]
        [InlineData("ML/DataSets/SMSSpamCollection", "\t")]
        [InlineData("ML/DataSets/SpamModel-Source.zip", ",")]
        public void FileSourceLoader_LoadData_Successfully(string fileName, string delimiter)
        {
            var modelName = "spamModel";

            var services = new ServiceCollection();

            services.AddSourceLoader<SpamInput, FileSourceLoader<SpamInput>>(modelName, options =>
            {
                options.Sources = new List<SourceLoaderFile<SpamInput>>
                {
                    new SourceLoaderFile<SpamInput>
                    {
                        FileName = fileName,
                        Delimiter = delimiter,
                        HasHeaderRecord = false
                    }
                };
            });

            var sp = services.BuildServiceProvider();

            var factory = sp.GetRequiredService<IOptionsFactory<SourceLoaderOptions<SpamInput>>>();

            var loader = factory.Create(modelName);

            var data = loader.SourceLoader.LoadData();

            Assert.Equal(5572, data.Count());
        }
    }
}
