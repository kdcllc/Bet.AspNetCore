using System.ComponentModel.DataAnnotations;

using Microsoft.Azure.Services.AppAuthentication;

namespace Bet.Extensions.DataProtection.AzureStorage
{
    public class DataProtectionAzureStorageOptions
    {
        /// <summary>
        ///  Azure Vault Key Id.
        ///  https://{name}.vault.azure.net/keys/DataProtectionKey/{id}.
        /// </summary>
        [Required]
        [Url]
        public string KeyVaultKeyId { get; set; } = string.Empty;

        /// <summary>
        /// Azure Storage Connection String.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// SAS Token used with Name of the Storage Account.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Azure Storage Name.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Container Name where to store the keys.
        /// </summary>
        [Required]
        public string ContainerName { get; set; } = string.Empty;

        /// <summary>
        /// The blob/file name of the keys.
        /// </summary>
        [Required]
        public string KeyBlobName { get; set; } = string.Empty;

        internal AzureServiceTokenProvider TokenProvider { get; set; } = new AzureServiceTokenProvider();
    }
}
