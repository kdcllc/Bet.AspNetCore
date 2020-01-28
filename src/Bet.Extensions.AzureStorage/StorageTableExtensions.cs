using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos.Table;

namespace Bet.Extensions.AzureStorage
{
    public static class StorageTableExtensions
    {
        /// <summary>
        /// Gets the Azure Storage Table entity based on Partition and Row Keys.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws exception if partition and row key are not provided.</exception>
        public static async Task<T> GetEntityAsync<T>(
            this CloudTable table,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentException("IsNullOrWhiteSpace", nameof(partitionKey));
            }

            if (string.IsNullOrWhiteSpace(rowKey))
            {
                throw new ArgumentException("IsNullOrWhiteSpace", nameof(rowKey));
            }

            return (T)(await table.ExecuteAsync(TableOperation.Retrieve<T>(partitionKey, rowKey), cancellationToken)).Result;
        }

        /// <summary>
        /// Executes and Gets Results.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<T> ExecuteAndGetResultAsync<T>(
            this CloudTable table,
            TableOperation operation,
            CancellationToken cancellationToken)
        {
            return (T)(await table.ExecuteAsync(operation, cancellationToken)).Result;
        }

        /// <summary>
        /// Executes Azure Storage Table Query.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the Azure Storage Table.</param>
        /// <param name="query">The query table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IList<T>> ExecuteQueryAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken? token = null;

            do
            {
                // ExecuteSegmentedAsync returns a maximum of 1000 entities.
                var seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || items.Count < query.TakeCount.Value));

            return items;
        }

        /// <summary>
        /// Executes Azure Storage Table Query with custom Process Result Action per each Query result.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the Azure Storage Table.</param>
        /// <param name="query">The query table.</param>
        /// <param name="processResult">The processing action for the result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IList<T>> ForEachQueryResultAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            Func<T, T> processResult,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken? token = null;

            var count = 0;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                foreach (var result in seg)
                {
                    var item = processResult(result);
                    items.Add(item);
                    count++;
                }
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || count < query.TakeCount.Value));

            return items;
        }

        public static async Task<TableContinuationToken?> ForEachQueryResultAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            TableContinuationToken token,
            Func<T, Task> processResultAsync,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var count = 0;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                foreach (var result in seg)
                {
                    await processResultAsync(result);
                    count++;
                }
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || count < query.TakeCount.Value));

            return token;
        }

        /// <summary>
        /// Executes a table query and processes the results as they arrive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="processResultAsync"></param>
        /// <param name="cancellationToken"></param>
        public static async Task ForEachQueryResultAsync<T>(
            this TableQuery<T> query,
            Func<T, Task> processResultAsync,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            TableContinuationToken? token = null;

            var count = 0;
            do
            {
                var seg = await query.ExecuteSegmentedAsync(token);
                token = seg.ContinuationToken;
                foreach (var result in seg)
                {
                    await processResultAsync(result);
                    count++;
                }
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || count < query.TakeCount.Value));
        }

        /// <summary>
        /// Executes a table query and processes the results as they arrive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="processResultAsync"></param>
        /// <param name="cancellationToken"></param>
        public static async Task ForEachQueryResultAsync<T>(
            this TableQuery<T> query,
            Func<T, Task<bool>> processResultAsync,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            TableContinuationToken? token = null;

            var count = 0;
            do
            {
                var seg = await query.ExecuteSegmentedAsync(token);
                token = seg.ContinuationToken;
                foreach (var result in seg)
                {
                    if (!await processResultAsync(result))
                    {
                        break;
                    }

                    count++;
                }
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || count < query.TakeCount.Value));
        }

        /// <summary>
        /// Executes a query and returns result. It fetches results in 1,000 count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IList<T>> ExecuteQueryAsync<T>(
            this TableQuery<T> query,
            CancellationToken cancellationToken = default) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken? token = null;

            do
            {
                var seg = await query.ExecuteSegmentedAsync(token, cancellationToken);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            }
            while (token != null && !cancellationToken.IsCancellationRequested && (query.TakeCount == null || items.Count < query.TakeCount.Value));

            return items;
        }

        /// <summary>
        /// Insert an Entity into the Azure Storage Table.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<T> InsertAsync<T>(
            this CloudTable table,
            T entity,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return (T)(await table.ExecuteAsync(TableOperation.Insert(entity), cancellationToken).ConfigureAwait(false)).Result;
        }

        /// <summary>
        /// Inserts or Merges the changes into the Azure Storage Table.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<T> InsertOrMergeAsync<T>(
            this CloudTable table,
            T entity,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return (T)(await table.ExecuteAsync(TableOperation.InsertOrMerge(entity), cancellationToken).ConfigureAwait(false)).Result;
        }

        /// <summary>
        /// Inserts or Replaces the values in Azure Storage Table.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<T> InsertOrReplaceAsync<T>(
            this CloudTable table,
            T entity,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return (T)(await table.ExecuteAsync(TableOperation.InsertOrReplace(entity), cancellationToken).ConfigureAwait(false)).Result;
        }

        /// <summary>
        /// Insert or Updates the Batch.
        /// </summary>
        /// <typeparam name="T">The type of the batch entities.</typeparam>
        /// <param name="table">The instance of the table.</param>
        /// <param name="entities">The list of entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<TableBatchResult> BatchInsertOrUpdateAsync<T>(
            this CloudTable table,
            IEnumerable<T> entities,
            CancellationToken cancellationToken) where T : ITableEntity, new()
        {
            var batchOperation = new TableBatchOperation();

            foreach (var item in entities)
            {
                batchOperation.InsertOrMerge(item);
            }

            return await table.ExecuteBatchAsync(batchOperation, cancellationToken);
        }

        /// <summary>
        /// Deletes Table Entity.
        /// </summary>
        /// <param name="table">The table instance.</param>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TableResult> DeleteAsync(
            this CloudTable table,
            TableEntity entity,
            CancellationToken cancellationToken)
        {
            return await table.ExecuteAsync(TableOperation.Delete(entity), cancellationToken);
        }
    }
}
