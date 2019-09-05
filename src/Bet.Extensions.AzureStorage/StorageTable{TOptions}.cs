using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.Logging;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.AzureStorage
{
    public class StorageTable<TOptions> : IStorageTable<TOptions> where TOptions : StorageTableOptions
    {
        private readonly Lazy<Task<CloudTable>> _table;
        private readonly ILogger<StorageTable<TOptions>> _logger;
        private readonly ExecutionLogger _executionLogger;

        public StorageTable(
            IOptionsMonitor<TOptions> storageTableOptions,
            IOptionsMonitor<StorageAccountOptions> storageAccountOptions,
            ILogger<StorageTable<TOptions>> logger)
        {
            var options = storageTableOptions.CurrentValue;
            var accountOptions = storageAccountOptions.Get(options.AzureStorageConfiguration);

            _table = new Lazy<Task<CloudTable>>(() => CreateOrGetBlobTable(options, accountOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _executionLogger = new ExecutionLogger(_logger);
        }

        /// <inheritdoc />
        public Task<CloudTable> Table => _table.Value;

        /// <inheritdoc />
        public Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateAsync<T>(
            IEnumerable<T> entities,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            return _executionLogger.ExecuteAndLogAsync(
                async () => await BatchInsertOrUpdateInternalAsync(entities, batchSize, cancellationToken),
                nameof(BatchInsertOrUpdateAsync));
        }

        /// <inheritdoc />
        public Task<TableResult> DeleteAsync(TableEntity entity, CancellationToken cancellationToken = default)
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () => await (await Table).DeleteAsync(entity, cancellationToken),
                nameof(DeleteAsync));
        }

        /// <inheritdoc />
        public Task<IList<T>> QueryAsync<T>(TableQuery<T> query, CancellationToken canellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () => await (await Table).ExecuteQueryAsync(query, canellationToken),
                nameof(QueryAsync));
        }

        public Task<IList<T>> QueryAsync<T>(
            TableQuery<T> query,
            Func<T, T> processResult,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () => await (await Table).ForEachQueryResultAsync(query, processResult, cancellationToken),
                nameof(QueryAsync));
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string partitionKey, string rowkey, CancellationToken cancellationToken = default)
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () =>
                {
                    var retrieveOperation = TableOperation.Retrieve(partitionKey, rowkey);
                    return await (await Table).ExecuteAndGetResultAsync<TableResult>(retrieveOperation, cancellationToken) != null;
                },
                nameof(ExistsAsync));
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () =>
                {
                    var retrieveOperation = TableOperation.Retrieve(entity.PartitionKey, entity.RowKey);
                    return await (await Table).ExecuteAndGetResultAsync<TableResult>(retrieveOperation, cancellationToken) != null;
                },
                nameof(ExistsAsync));
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () => await (await Table).GetEntityAsync<T>(partitionKey, rowKey, cancellationToken),
                nameof(GetAsync));
        }

        /// <inheritdoc />
        public Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
                async () => await (await Table).InsertAsync(entity, cancellationToken),
                nameof(InsertAsync));
        }

        /// <inheritdoc />
        public Task<T> InsertOrReplaceAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
               async () => await (await Table).InsertOrReplaceAsync(entity, cancellationToken),
               nameof(InsertOrReplaceAsync));
        }

        /// <inheritdoc />
        public Task<T> InsertOrUpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return _executionLogger.ExecuteAndLogAsync(
               async () => await (await Table).InsertOrMergeAsync(entity, cancellationToken),
               nameof(InsertOrUpdateAsync));
        }

        private async Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateInternalAsync<T>(
            IEnumerable<T> entities,
            int batchSize,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            var tableBatchResults = new List<TableBatchResult>();

            var processCount = 0;
            var exCount = 0;

            try
            {
                var itemCount = entities.Count();
                var finalBatchSize = itemCount > batchSize ? batchSize : itemCount;

                foreach (var batch in entities.Batch<T>(finalBatchSize))
                {
                    try
                    {
                        Interlocked.Add(ref processCount, finalBatchSize);
                        _logger?.LogDebug($"Process {processCount} of {itemCount}");

                        // Execute the operation.
                        var result = await (await Table).BatchInsertOrUpdateAsync<T>(batch, cancellationToken);

                        if (result != null)
                        {
                            tableBatchResults.Add(result);
                        }
                    }
                    catch (StorageException ex)
                    {
                        Interlocked.Increment(ref exCount);
                        _logger?.LogError(ex.GetBaseException().ToString());

                        // throw;
                    }
                }
            }
            catch (AggregateException ex)
            {
                _logger?.LogError("Error occurred with the batch update of the entities", ex?.InnerException?.GetBaseException());
            }

            _logger?.LogDebug("Succeeded count {success}: Failed count {failed}", processCount, exCount);

            return tableBatchResults;
        }

        private async Task<CloudTable> CreateOrGetBlobTable(
            StorageTableOptions options,
            StorageAccountOptions storageOptions,
            CancellationToken cancellationToken = default)
        {
            var storageAccount = CreateStorageAccountFromConnectionString(storageOptions.ConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            var table = tableClient.GetTableReference(options.TableName);
            if (await table.CreateIfNotExistsAsync(cancellationToken))
            {
                _logger?.LogInformation("Created Table named: {0}", options.TableName);
            }
            else
            {
                _logger?.LogInformation("Table {0} already exists", options.TableName);
            }

            return table;
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            try
            {
                if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
                {
                    return storageAccount;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);

                throw;
            }
        }
    }
}
