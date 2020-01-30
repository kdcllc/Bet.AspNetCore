using Microsoft.Azure.Storage.Blob;

namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageBlobOptions : StorageOptionsBase
    {
        /// <summary>
        /// Azure Storage Blob Container Name.
        /// </summary>
        public string ContainerName { get; set; } = string.Empty;

        /// <summary>
        /// If container doesn't exist it will be created with this permission. The default value is 'Off'.
        ///
        /// No public access. Only the account owner can read resources in this container.
        /// Off = 0,
        ///
        /// Container-level public access. Anonymous clients can read container and blob data.
        /// Container = 1,
        ///
        /// Blob-level public access. Anonymous clients can read blob data within this container, but not container data.
        /// Blob = 2,
        ///
        /// Unknown access type.
        /// Unknown = 3.
        /// </summary>
        public BlobContainerPublicAccessType PublicAccessType { get; set; } = BlobContainerPublicAccessType.Off;
    }
}
