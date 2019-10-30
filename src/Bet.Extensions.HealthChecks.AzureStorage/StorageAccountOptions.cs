using Microsoft.Azure.Services.AppAuthentication;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class StorageAccountOptions
    {
        /// <summary>
        /// Azure Storage Connection String.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Name of the Storage Account. Used with <see cref="AzureServiceTokenProvider"/>.
        /// This is enable only if ConnectionString is empty or null.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SAS Token used with Name of the Storage Account.
        /// </summary>
        public string Token { get; set; }

        internal string ContainerName { get; set; }
    }
}
