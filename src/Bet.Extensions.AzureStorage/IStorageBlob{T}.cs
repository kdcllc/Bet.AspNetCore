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
        string Name { get; }

        Task<string> AddAsync(byte[] content, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        Task<string> AddAsync(object item, string blobId = null, CancellationToken cancellationToken = default);

        Task<string> AddAsync(object item, string blobId, Encoding encoding, CancellationToken cancellationToken = default);

        Task<string> AddAsync(Stream content, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        Task<string> AddAsync(Uri sourceUri, string blobId = null, string contentType = null, CancellationToken cancellationToken = default);

        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, int batchSize = 25, CancellationToken cancellationToken = default);

        Task<IList<string>> AddBatchAsync<T>(IEnumerable<T> items, Encoding encoding, int batchSize = 25, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string blobName, CancellationToken cancellationToken = default);

        Task<IEnumerable<CloudBlockBlob>> GetAllAsync(string prefix = default, CancellationToken cancellationToken = default);

        Task<Stream> GetAsync(string blobName, CancellationToken cancellationToken = default);

        Task<T> GetAsync<T>(string blobName, CancellationToken cancellationToken = default);

        Task<byte[]> GetBytesAsync(string blobName, CancellationToken cancellationToken = default);

        Task SaveAsync(byte[] data, string pathLocation, string fileName, FileMode mode = FileMode.Create, CancellationToken cancellationToken = default);
    }
}
