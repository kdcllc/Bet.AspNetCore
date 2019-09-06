using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Cosmos.Table;

namespace Bet.Extensions.AzureStorage
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-patterns.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IStorageTable<TOptions> where TOptions : StorageTableOptions
    {
        /// <summary>
        /// The instance of Azure Storage Table.
        /// </summary>
        Task<CloudTable> Table { get; }

        /// <summary>
        /// Batch Insert or Update.
        /// </summary>
        /// <typeparam name="T">The type of the object to persist.</typeparam>
        /// <param name="entities">The list of objects to persist.</param>
        /// <param name="batchSize">The size of the batch to process. The default is 100 items.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>boolean.</returns>
        Task<IEnumerable<TableBatchResult>> BatchInsertOrUpdateAsync<T>(IEnumerable<T> entities, int batchSize = 100, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Checks if the item exist based on key, and row key parameters.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowkey">The row key to verify.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>boolean.</returns>
        Task<bool> ExistsAsync(string partitionKey, string rowkey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the entity exist based on the entity object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be checked.</typeparam>
        /// <param name="entity">The instance of the object to verify.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>boolean.</returns>
        Task<bool> ExistsAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Get the object based on partition key and row key combinations.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Single instance of the object is returned.</returns>
        Task<T> GetAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Insert object into Azure Storage table.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert.</typeparam>
        /// <param name="entity">The instance of the object to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Inserts or Updates the object in the Azure Storage Table.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert or update.</typeparam>
        /// <param name="entity">The instance of the object to insert or update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> InsertOrUpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Inserts or Replaces the object in the Azure Storage Table.
        /// The Replace method of the TableOperation class always replaces the complete entity in the Table service.
        /// If you do not include a property in the request when that property exists in the stored entity,
        /// the request removes that property from the stored entity.
        /// Unless you want to remove a property explicitly from a stored entity, you must include every property in the request.
        /// </summary>
        /// <typeparam name="T">The type of the object to insert or replace.</typeparam>
        /// <param name="entity">The instance of the object to insert or replace.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> InsertOrReplaceAsync<T>(T entity, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Deletes Table Entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TableResult> DeleteAsync(TableEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes Azure Storage Table Query.
        /// </summary>
        /// <typeparam name="T">The type of the object to return from the query.</typeparam>
        /// <param name="query">The query based on Azure Storage Table syntax.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IList<T>> QueryAsync<T>(TableQuery<T> query, CancellationToken cancellationToken = default) where T : ITableEntity, new();

        /// <summary>
        /// Executes Azure Storage Table Query with custom Process Result Action per each Query result.
        /// The max result is 1,000 records.
        /// </summary>
        /// <typeparam name="T">The type of the object to return from the query.</typeparam>
        /// <param name="query">The query based on Azure Storage Table syntax.</param>
        /// <param name="processResult">The processing action for the result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IList<T>> QueryAsync<T>(TableQuery<T> query, Func<T, T> processResult, CancellationToken cancellationToken = default) where T : ITableEntity, new();
    }
}
