using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <inheritdoc />
    public class StorageTable<TOptions> : IStorageTable<TOptions> where TOptions : StorageTableOptions
    {
        private readonly ILogger<StorageTable<TOptions>> _logger;
        private readonly ExecutionLogger _executionLogger;

        private readonly ConcurrentDictionary<string, Task<CloudTable>> _namedTables = new ConcurrentDictionary<string, Task<CloudTable>>();
        private readonly IOptionsMonitor<TOptions> _storageTableOptionsMonitor;
        private readonly IOptionsMonitor<StorageAccountOptions> _storageAccountOptionsMonitor;

        public StorageTable(
            IOptionsMonitor<TOptions> storageTableOptionsMonitor,
            IOptionsMonitor<StorageAccountOptions> storageAccountOptionsMonitor,
            ILogger<StorageTable<TOptions>> logger)
        {
            _storageTableOptionsMonitor = storageTableOptionsMonitor ?? throw new ArgumentNullException(nameof(storageTableOptionsMonitor));
            _storageAccountOptionsMonitor = storageAccountOptionsMonitor ?? throw new ArgumentNullException(nameof(storageAccountOptionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _executionLogger = new ExecutionLogger(_logger);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateAsync<T>(
            string named,
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
                async () => await BatchInsertOrUpdateInternalAsync(named, entities, batchSize, cancellationToken),
                nameof(BatchInsertOrUpdateAsync));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TableBatchResult>> DeleteBatchAsync<T>(
            string named,
            TableQuery<T> query,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var tasks = new List<Task<TableBatchResult>>();

            var table = await GetNamedTable(named, cancellationToken);

            var entities = await table.ExecuteQueryAsync<T>(query, cancellationToken);
            var offset = 0;
            while (offset < entities.Count)
            {
                var batch = new TableBatchOperation();

                var rows = entities.Skip(offset).Take(batchSize).ToList();
                rows.ForEach(row => batch.Delete(row));

                var result = table.ExecuteBatchAsync(batch, cancellationToken);

                tasks.Add(result);

                offset += rows.Count;
            }

            return await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TableBatchResult>> DeleteBatchAsync<T>(
            TableQuery<T> query,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            return DeleteBatchAsync<T>(string.Empty, query, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TableResult> DeleteAsync(
            string named,
            TableEntity entity,
            CancellationToken cancellationToken = default)
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () => await table.DeleteAsync(entity, cancellationToken),
                nameof(DeleteAsync));
        }

        /// <inheritdoc />
        public async Task<IList<T>> QueryAsync<T>(
            string named,
            TableQuery<T> query,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () => await table.ExecuteQueryAsync(query, cancellationToken),
                nameof(QueryAsync));
        }

        /// <inheritdoc />
        public async Task<IList<T>> QueryAsync<T>(
            string named,
            TableQuery<T> query,
            Func<T, T> processResult,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () => await table.ForEachQueryResultAsync(query, processResult, cancellationToken),
                nameof(QueryAsync));
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(
            string named,
            string partitionKey,
            string rowkey,
            CancellationToken cancellationToken = default)
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () =>
                {
                    var retrieveOperation = TableOperation.Retrieve(partitionKey, rowkey);
                    return await table.ExecuteAndGetResultAsync<TableResult>(retrieveOperation, cancellationToken) != null;
                },
                nameof(ExistsAsync));
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () =>
                {
                    var retrieveOperation = TableOperation.Retrieve(entity.PartitionKey, entity.RowKey);
                    return await table.ExecuteAndGetResultAsync<TableResult>(retrieveOperation, cancellationToken) != null;
                },
                nameof(ExistsAsync));
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            string named,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () => await table.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken),
                nameof(GetAsync));
        }

        /// <inheritdoc />
        public async Task<T> InsertAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
                async () => await table.InsertAsync(entity, cancellationToken),
                nameof(InsertAsync));
        }

        /// <inheritdoc />
        public async Task<T> InsertOrReplaceAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
               async () => await table.InsertOrReplaceAsync(entity, cancellationToken),
               nameof(InsertOrReplaceAsync));
        }

        /// <inheritdoc />
        public async Task<T> InsertOrUpdateAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var table = await GetNamedTable(named, cancellationToken);

            return await _executionLogger.ExecuteAndLogAsync(
               async () => await table.InsertOrMergeAsync(entity, cancellationToken),
               nameof(InsertOrUpdateAsync));
        }

        public Task<CloudTable> GetNamedTable(string namedTable, CancellationToken cancellationToken = default)
        {
            if (_namedTables.TryGetValue(namedTable, out var container))
            {
                return container;
            }

            var options = _storageTableOptionsMonitor.Get(namedTable);
            var storageOptions = _storageAccountOptionsMonitor.Get(options.AccountName);

            var createdContainer = Task.Run(() => CreateOrGetBlobTable(options, storageOptions, cancellationToken));

            _namedTables.AddOrUpdate(namedTable, createdContainer, (_, __) => createdContainer);

            return createdContainer;
        }

        private async Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateInternalAsync<T>(
            string named,
            IEnumerable<T> entities,
            int batchSize,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            var tasks = new List<Task<TableBatchResult>>();

            var table = await GetNamedTable(named, cancellationToken);

            var offset = 0;

            while (offset < entities.Count())
            {
                var rows = entities.Skip(offset).Take(batchSize).ToList();

                var result = table.BatchInsertOrUpdateAsync<T>(rows, cancellationToken);

                tasks.Add(result);
                offset += rows.Count;
            }

            return await Task.WhenAll(tasks);
        }

        private async Task<CloudTable> CreateOrGetBlobTable(
            StorageTableOptions options,
            StorageAccountOptions storageOptions,
            CancellationToken cancellationToken = default)
        {
            var sw = ValueStopwatch.StartNew();

            var storageAccount = CreateStorageAccountFromConnectionString(storageOptions.ConnectionString);

            if (storageAccount == null)
            {
                throw new NullReferenceException($"{nameof(storageAccount)} wasn't created please make sure Connection String is provided.");
            }

            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            var table = tableClient.GetTableReference(options.TableName);
            if (await table.CreateIfNotExistsAsync(cancellationToken))
            {
                _logger.LogInformation("[Azure Table] No Azure Table [{tableName}] found - so one was auto created.", options.TableName);
            }
            else
            {
                _logger.LogInformation("[Azure Table] Using existing Azure Table:[{tableName}].", options.TableName);
            }

            _logger.LogInformation("[Azure Table][{methodName}] Elapsed: {seconds}sec", nameof(CreateOrGetBlobTable), sw.GetElapsedTime().TotalSeconds);

            return table;
        }

        private CloudStorageAccount? CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            try
            {
                if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
                {
                    return storageAccount;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
            }

            return null;
        }
    }
}
