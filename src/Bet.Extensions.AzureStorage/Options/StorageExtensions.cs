using System;

using Microsoft.Azure.Storage.Blob;

namespace Bet.Extensions.AzureStorage.Options
{
    public static class StorageExtensions
    {
        public static CloudBlobContainer GetContainerRef(
            this StorageAccountOptions options,
            string containerName)
        {
            if (options.CloudStorageAccount == null)
            {
                throw new ArgumentNullException(nameof(options), "CloudStorageAccount can't be null");
            }

            var storageAccount = options.CloudStorageAccount.Value.GetAwaiter().GetResult();

            var client = storageAccount.CreateCloudBlobClient();

            return client.GetContainerReference(containerName);
        }
    }
}
