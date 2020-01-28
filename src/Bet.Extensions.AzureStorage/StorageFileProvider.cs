using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.AzureStorage
{
    public class StorageFileProvider : IFileProvider
    {
        private readonly CloudBlobContainer _container;

        public StorageFileProvider(StorageAccountOptions options, string containerName)
        {
            var storageAccount = options.CloudStorageAccount?.Value.GetAwaiter().GetResult();

            var blobClient = storageAccount.CreateCloudBlobClient();

            _container = blobClient.GetContainerReference(containerName);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var blob = _container.GetDirectoryReference(subpath.TrimStart('/').TrimEnd('/'));
            return new StorageDirectoryContents(blob);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var blob = _container.GetBlockBlobReference(subpath.TrimStart('/').TrimEnd('/'));
            return new StorageFileInfo(blob);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
