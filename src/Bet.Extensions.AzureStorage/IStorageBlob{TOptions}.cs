using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;

namespace Bet.Extensions.AzureStorage
{
    /// <summary>
    /// The interface for Azure Storage Blob interactions.
    /// </summary>
    /// <typeparam name="TOptions">The type of the configurations to be used for the instance.</typeparam>
    public interface IStorageBlob<TOptions> where TOptions : StorageBlobOptions
    {
        /// <summary>
        /// The Azure Storage Blob Container name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Adds an object to an Azure Blob Storage, as byte[].
        /// </summary>
        /// <param name="content">The array of bytes to be stored in Azure Storage Blob.</param>
        /// <param name="blobId">Optional. The identifier/ name for the content to be stored on Azure. If not supplied,  then a new. <code>Guid</code> will be used.</param>
        /// <param name="contentType">Optional. What type of content exists in the file. If none is provided, then Azure defaults this value to. <code>application/octet-stream</code>.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>string: blob name.</returns>
        Task<string> AddAsync(byte[] content, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to an Azure Blob Storage, as object type by serializing it to json and then storing it as json.
        /// </summary>
        /// <param name="item">The item to be stored as object type.</param>
        /// <param name="blobId">Optional. The identifier/ name for the content to be stored on Azure. If not supplied,  then a new. <code>Guid</code> will be used.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>string: blob name.</returns>
        Task<string> AddAsync(object item, string blobId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to an Azure Blob Storage, as object type by serializing it to json and then storing it as json.
        /// </summary>
        /// <param name="item">The item to be stored as object type.</param>
        /// <param name="blobId">Optional. The identifier/ name for the content to be stored on Azure. If not supplied,  then a new. <code>Guid</code> will be used.</param>
        /// <param name="encoding">The encoding type to serialize the. <code>item</code>.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>string: blob name.</returns>
        Task<string> AddAsync(object item, string blobId, Encoding encoding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to an Azure Storage Blob via System.IO.Stream.
        /// </summary>
        /// <param name="content">The System.IO.Stream object instance.</param>
        /// <param name="blobId">Optional. The identifier/ name for the content to be stored on Azure. If not supplied,  then a new. <code>Guid</code> will be used.</param>
        /// <param name="contentType">Optional. What type of content exists in the file. If none is provided, then Azure defaults this value to. <code>application/octet-stream</code>.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>string: blob name.</returns>
        Task<string> AddAsync(Stream content, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to Azure Storage Blob from a URI.
        /// </summary>
        /// <param name="sourceUri">The source URI to upload.</param>
        /// <param name="blobId">Optional. The identifier/name of this content to be stored on Azure. If not supplied, then a new. <code>Guid</code> will be used.</param>
        /// <param name="contentType">Optional. What type of content exists in the file. If none is provided, then Azure defaults this value to. <code>application/octet-stream</code>.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>string: blob name.</returns>
        Task<string> AddAsync(Uri sourceUri, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds collection of objects to Azure Storage Blob as serialized the objects to json.
        /// </summary>
        /// <typeparam name="T">The type of the object in the collection.</typeparam>
        /// <param name="items">The list of the items.</param>
        /// <param name="batchSize">The size of the batch to process at one time.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>list of strings: blob name.</returns>
        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds collection of objects to Azure Storage Blob as serialized the objects to json.
        /// </summary>
        /// <typeparam name="T">The type of the object in the collection.</typeparam>
        /// <param name="items">The list of the items.</param>
        /// <param name="encoding">The encoding type to serialize the. <code>item</code>.</param>
        /// <param name="batchSize">The size of the batch to process at one time.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>list of strings: blob name.</returns>
        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, Encoding encoding, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes Azure Storage Blob based on name/identifier.
        /// </summary>
        /// <param name="blobName">The name/identifier of the blob.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>task bool: result. </returns>
        Task<bool> DeleteAsync(string blobName, CancellationToken cancellationToken = default);

        Task<IEnumerable<CloudBlockBlob>> GetAllAsync(string prefix = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the single object from Azure Storage Blob.
        /// </summary>
        /// <param name="blobName">The name/identifier of the blob.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>Sytem.IO.Stream: object.</returns>
        Task<Stream> GetAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the single serialized json as T type object.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="blobName">The name/identifier of the blob.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>Sytem.IO.Stream: object.</returns>
        Task<T> GetAsync<T>(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an object as array of bytes.
        /// </summary>
        /// <param name="blobName">The name/identifier of the blob.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>byte[]: object.</returns>
        Task<byte[]> GetBytesAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves array of bytes to a local OS file location.
        /// </summary>
        /// <param name="data">The object as array of bytes.</param>
        /// <param name="pathLocation">The file location on the machine.</param>
        /// <param name="fileName">The file name to be used to save.</param>
        /// <param name="mode">The mode of FileMode.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>task: void.</returns>
        Task SaveAsync(byte[] data, string pathLocation, string fileName, FileMode mode = FileMode.Create, CancellationToken cancellationToken = default);
    }
}
