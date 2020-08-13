using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection.AzureStorage;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Azure.Services.AppAuthentication;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.DataProtection.AzureStorage
{
    public class KeyManagementOptionsSetup : IPostConfigureOptions<KeyManagementOptions>
    {
        private readonly ILogger<KeyManagementOptionsSetup> _logger;
        private readonly IOptions<DataProtectionAzureStorageOptions> _dataProtectionOptions;

        public KeyManagementOptionsSetup(
            IOptions<DataProtectionAzureStorageOptions> dataProtectionOptions,
            ILogger<KeyManagementOptionsSetup> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataProtectionOptions = dataProtectionOptions;
        }

        public void PostConfigure(string name, KeyManagementOptions options)
        {
            var cloudStorageAccount = GetStorageAccountAsync(_dataProtectionOptions.Value).GetAwaiter().GetResult();
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = CreateCloudBlobContainer(_dataProtectionOptions.Value, cloudStorageAccount).GetAwaiter().GetResult();
            var blob = cloudBlobContainer.GetBlockBlobReference(_dataProtectionOptions.Value.KeyBlobName);

            options.XmlRepository = new AzureBlobXmlRepository(() => blob);
        }

        private async Task<NewTokenAndFrequency> TokenRenewerAsync(object state, CancellationToken cancellationToken)
        {
            var sw = ValueStopwatch.StartNew();

            cancellationToken.ThrowIfCancellationRequested();

            var (authResult, next) = await GetToken(state);

            _logger.LogInformation("[Azure Storage][DataProtection] Authentication Elapsed: {elapsed}sec", sw.GetElapsedTime().TotalSeconds);

            if (next.Ticks < 0)
            {
                _logger.LogInformation("[Azure Storage][DataProtection] Authentication Renewing Token...");

                var swr = ValueStopwatch.StartNew();

                (authResult, next) = await GetToken(state);

                _logger.LogInformation("[Azure Storage][DataProtection] Authentication Renewing Token Elapsed: {elapsed}", swr.GetElapsedTime().TotalSeconds);
            }

            // Return the new token and the next refresh time.
            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }

        // https://github.com/MicrosoftDocs/azure-docs/blob/941ccc038829a0be5fa8515ffba9956cfc02a5e6/articles/storage/common/storage-auth-aad-msi.md
        private async Task<(AppAuthenticationResult result, TimeSpan ticks)> GetToken(object state)
        {
            // Specify the resource ID for requesting Azure AD tokens for Azure Storage.
            const string StorageResource = "https://storage.azure.com/";

            // Use the same token provider to request a new token.
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync(StorageResource);

            // Renew the token 5 minutes before it expires.
            var next = authResult.ExpiresOn - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5);

            // debug purposes
            // var next = (authResult.ExpiresOn - authResult.ExpiresOn) + TimeSpan.FromSeconds(15);
            return (authResult, next);
        }

        private async Task<CloudStorageAccount> GetStorageAccountAsync(
            DataProtectionAzureStorageOptions options,
            CancellationToken cancellationToken = default)
        {
            CloudStorageAccount account;

            if (!string.IsNullOrEmpty(options.ConnectionString)
                && CloudStorageAccount.TryParse(options.ConnectionString, out var cloudStorageAccount))
            {
                account = cloudStorageAccount;

                _logger.LogInformation("[Azure Storage][DataProtection] Authenticating with ConnectionString.");
            }
            else if (!string.IsNullOrEmpty(options.Name)
                && string.IsNullOrEmpty(options.Token))
            {
                // Get the initial access token and the interval at which to refresh it.
                var azureServiceTokenProvider = options.TokenProvider;
                var tokenAndFrequency = await TokenRenewerAsync(
                    azureServiceTokenProvider,
                    cancellationToken);

                // Create storage credentials using the initial token, and connect the callback function
                // to renew the token just before it expires
#pragma warning disable CA2000 // Dispose objects before losing scope
                var tokenCredential = new TokenCredential(
                    tokenAndFrequency.Token,
                    TokenRenewerAsync,
                    azureServiceTokenProvider,
#pragma warning disable CS8629 // Nullable value type may be null.
                    tokenAndFrequency.Frequency.Value);
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning restore CA2000 // Dispose objects before losing scope

                var storageCredentials = new StorageCredentials(tokenCredential);

                account = new CloudStorageAccount(storageCredentials, options.Name, string.Empty, true);

                _logger.LogInformation("[Azure Storage][DataProtection] Authenticating with MSI Token.");
            }
            else if (!string.IsNullOrEmpty(options.Name)
                && !string.IsNullOrEmpty(options.Token))
            {
                account = new CloudStorageAccount(new StorageCredentials(options.Token), options.Name, true);
                _logger.LogInformation("[Azure Storage][DataProtection] Authenticating with SAS Token.");
            }
            else
            {
                throw new ArgumentException($"One of the following must be set: '{options.ConnectionString}' or '{options.Name}'!");
            }

            return account;
        }

        private async Task<CloudBlobContainer> CreateCloudBlobContainer(
            DataProtectionAzureStorageOptions options,
            CloudStorageAccount cloudStorageAccount,
            CancellationToken cancellationToken = default)
        {
            var sw = ValueStopwatch.StartNew();

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(options.ContainerName);

            var created = await cloudBlobContainer.CreateIfNotExistsAsync(cancellationToken);

            if (created)
            {
                _logger.LogInformation("[Azure Blob][DataProtection] No Azure Blob [{blobName}] found - so one was auto created.", options.ContainerName);
            }
            else
            {
                _logger.LogInformation("[Azure Blob][DataProtection] Using existing Azure Blob:[{blobName}].", options.ContainerName);
            }

            _logger.LogInformation("[Azure Blob][DataProtection] Completed: {methodName}; Elapsed: {elapsed}sec", nameof(CreateCloudBlobContainer), sw.GetElapsedTime().TotalSeconds);

            return cloudBlobContainer;
        }
    }
}
