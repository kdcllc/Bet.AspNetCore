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
    public interface IStorageBlob<TOptions> where TOptions : StorageBlobOptions
    {
        /// <summary>
        /// Adds byte array content to Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If file fails to be added.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
        Task<string> AddAsync(string named, byte[] content, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds byte array content to Azure Blob Container.
        /// </summary>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If file fails to be added.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
        Task<string> AddAsync(byte[] content, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the object to Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string> AddAsync(string named, object item, string? blobId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the object to Azure Blob Container.
        /// </summary>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string> AddAsync(object item, string? blobId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="encoding">The encoding type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string> AddAsync(string named, object item, string blobId, Encoding encoding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an object to Azure Blob Container.
        /// </summary>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="encoding">The encoding type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string> AddAsync(object item, string blobId, Encoding encoding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds Steam content to Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string?> AddAsync(string named, Stream content, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds Steam content to Azure Blob Container.
        /// </summary>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string?> AddAsync(Stream content, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds data from URI to Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="sourceUri"></param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string?> AddAsync(string named, Uri sourceUri, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds data from URI to Azure Blob Container.
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<string?> AddAsync(Uri sourceUri, string? blobId = null, string? contentType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the array of object to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="items">The array of objects.</param>
        /// <param name="batchSize">The batch size. The default is 25.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IList<string>> AddBatchAsync<T>(string named, IEnumerable<T> items, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the array of object to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The array of objects.</param>
        /// <param name="batchSize">The batch size. The default is 25.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds array of {T} objects to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="items">The array of {T} objects.</param>
        /// <param name="encoding">The encoding to be used.</param>
        /// <param name="batchSize">The batch size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IList<string>> AddBatchAsync<T>(string named, IEnumerable<T> items, Encoding encoding, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds array of {T} objects to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The array of {T} objects.</param>
        /// <param name="encoding">The encoding to be used.</param>
        /// <param name="batchSize">The batch size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, Encoding encoding, int batchSize = 25, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the Blob from Azure Blob Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<bool> DeleteAsync(string named, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the Blob from Azure Blob Container.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<bool> DeleteAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all blobs in the container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="prefix">The prefix to be used for the search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudBlockBlob>> GetAllAsync(string named, string prefix = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all blobs in the container.
        /// </summary>
        /// <param name="prefix">The prefix to be used for the search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudBlockBlob>> GetAllAsync(string prefix = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single blob as stream based on blob's name.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        Task<Stream?> GetAsync(string named, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single blob as stream based on blob's name.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        Task<Stream?> GetAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get serialized type of the blob based on the name.
        /// </summary>
        /// <typeparam name="T">The type of the serialized object.</typeparam>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        Task<T> GetAsync<T>(string named, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get serialized type of the blob based on the name.
        /// </summary>
        /// <typeparam name="T">The type of the serialized object.</typeparam>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        Task<T> GetAsync<T>(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets <see cref="CloudBlockBlob"/>.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<CloudBlockBlob?> GetBlobAsync(string named, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets <see cref="CloudBlockBlob"/>.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<CloudBlockBlob?> GetBlobAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets blob bytes based on the blob name in the container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<byte[]?> GetBytesAsync(string named, string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets blob bytes based on the blob name in the container.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<byte[]?> GetBytesAsync(string blobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets Named Container.
        /// </summary>
        /// <param name="named">The name of the container options that were registered.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Lazy<Task<CloudBlobContainer>> GetNamedContainer(string named, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the byte array to the OS file system.
        /// </summary>
        /// <param name="data">The byte array blob.</param>
        /// <param name="pathLocation">The location on OS.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task SaveAsync(byte[] data, string pathLocation, string fileName, FileMode mode = FileMode.Create, CancellationToken cancellationToken = default);
    }
}
