using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Cosmos.Table;

namespace Bet.Extensions.AzureStorage
{
    /// <summary>
    /// Azure Storage Table wrapper interface.
    /// based on the ideas from https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-patterns.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IStorageTable<TOptions> where TOptions : StorageTableOptions
    {
        /// <summary>
        /// Batch Insert or Update.
        /// </summary>
        /// <typeparam name="T">The type of the object to persist.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="entities">The list of objects to persist.</param>
        /// <param name="batchSize">The size of the batch to process. The default is 100 items.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>boolean.</returns>
        Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateAsync<T>(
            string named,
            IEnumerable<T> entities,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Checks if the item exist based on key, and row key parameters.
        /// </summary>
        /// <param name="named">The name of the options.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowkey">The row key to verify.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>boolean.</returns>
        Task<bool> ExistsAsync(
            string named,
            string partitionKey,
            string rowkey,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the entity exist based on the entity object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be checked.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="entity">The instance of the object to verify.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>boolean.</returns>
        Task<bool> ExistsAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Get the object based on partition key and row key combinations.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Single instance of the object is returned.</returns>
        Task<T> GetAsync<T>(
            string named,
            string partitionKey,
            string rowKey,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Insert object into Azure Storage table.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="entity">The instance of the object to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> InsertAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Inserts or Updates the object in the Azure Storage Table.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert or update.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="entity">The instance of the object to insert or update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> InsertOrUpdateAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Inserts or Replaces the object in the Azure Storage Table.
        /// The Replace method of the TableOperation class always replaces the complete entity in the Table service.
        /// If you do not include a property in the request when that property exists in the stored entity,
        /// the request removes that property from the stored entity.
        /// Unless you want to remove a property explicitly from a stored entity, you must include every property in the request.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert or replace.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="entity">The instance of the object to insert or replace.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> InsertOrReplaceAsync<T>(
            string named,
            T entity,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Deletes Table Entity.
        /// </summary>
        /// <param name="named">The name of the options.</param>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TableResult> DeleteAsync(
            string named,
            TableEntity entity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes Azure Storage Table Query.
        /// </summary>
        /// <typeparam name="T">The type of the object to return from the query.</typeparam>
        /// <param name="named">The name of the options.</param>
        /// <param name="query">The query based on Azure Storage Table syntax.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IList<T>> QueryAsync<T>(
            string named,
            TableQuery<T> query,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Executes Azure Storage Table Query with custom Process Result Action per each Query result.
        /// The max result is 1,000 records.
        /// </summary>
        /// <typeparam name="T">The type of the object to return from the query.</typeparam>
        /// <param name="name">The name of the options.</param>
        /// <param name="query">The query based on Azure Storage Table syntax.</param>
        /// <param name="processResult">The processing action for the result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IList<T>> QueryAsync<T>(
            string name,
            TableQuery<T> query,
            Func<T, T> processResult,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Gets <see cref="CloudTable"/> object.
        /// </summary>
        /// <param name="namedTable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CloudTable> GetNamedTable(string namedTable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes batch records based on the query result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named"></param>
        /// <param name="query"></param>
        /// <param name="batchSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<TableBatchResult>> DeleteBatchAsync<T>(
            string named,
            TableQuery<T> query,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Deletes batch records based on the query result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="batchSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<TableBatchResult>> DeleteBatchAsync<T>(
            TableQuery<T> query,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : ITableEntity, new();
    }
}
