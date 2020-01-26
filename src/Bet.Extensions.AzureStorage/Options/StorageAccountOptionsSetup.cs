using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageAccountOptionsSetup :
        IConfigureNamedOptions<StorageAccountOptions>,
        IPostConfigureOptions<StorageAccountOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StorageAccountOptionsSetup> _logger;

        private StorageAccountOptions _options;

        public StorageAccountOptionsSetup(
            IConfiguration configuration,
            ILogger<StorageAccountOptionsSetup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void Configure(StorageAccountOptions options)
        {
            var configPath = GetRootSectionPath(options);
            configPath = ConfigurationPath.Combine(configPath, $"{options.OptionName}Account");

            Log.DebugInfo(_logger, nameof(Configure), configPath);

            var section = _configuration.GetSection(configPath);
            section.Bind(options);
        }

        public void Configure(string name, StorageAccountOptions options)
        {
            options.OptionName = name;
            Configure(options);
        }

        public void PostConfigure(string name, StorageAccountOptions options)
        {
            _options = options;

            if (options.CloudStorageAccount == null)
            {
                options.CloudStorageAccount = new Lazy<Task<CloudStorageAccount>>(() => GetStorageAccountAsync());
            }
        }

        private string GetRootSectionPath(StorageAccountOptions options)
        {
            return _configuration.GetSection(options.RootSectionName).Exists() ? options.RootSectionName : options.GetType().Name;
        }

        private async Task<NewTokenAndFrequency> TokenRenewerAsync(object state, CancellationToken cancellationToken)
        {
            var sw = ValueStopwatch.StartNew();

            var (authResult, next) = await GetToken(state);

            _logger.LogInformation("[Azure Storage][Authentication] Eclipsed: {elapsed}sec", sw.GetElapsedTime().TotalSeconds);

            if (next.Ticks < 0)
            {
                next = default;

                _logger.LogInformation("[Azure Storage][Authentication Renewing Token] Started...");

                var swr = ValueStopwatch.StartNew();

                (authResult, next) = await GetToken(state);

                _logger.LogInformation("[Azure Storage][Authentication Renewing Token] Elapsed: {elapsed}sec", swr.GetElapsedTime().TotalSeconds);
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

        private async Task<CloudStorageAccount> GetStorageAccountAsync()
        {
            CloudStorageAccount account;

            if (_options.ConnectionString != null && CloudStorageAccount.TryParse(_options.ConnectionString, out var cloudStorageAccount))
            {
                account = cloudStorageAccount;

                _logger.LogInformation("[Azure Storage][Authentication] Using ConnectionString.");
            }
            else if (_options.Name != null
                && string.IsNullOrEmpty(_options.Token))
            {
                // Get the initial access token and the interval at which to refresh it.
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var tokenAndFrequency = await TokenRenewerAsync(
                    azureServiceTokenProvider,
                    CancellationToken.None);

                // Create storage credentials using the initial token, and connect the callback function
                // to renew the token just before it expires
#pragma warning disable CA2000 // Dispose objects before losing scope
                var tokenCredential = new TokenCredential(
                    tokenAndFrequency.Token,
                    TokenRenewerAsync,
                    azureServiceTokenProvider,
                    tokenAndFrequency.Frequency.Value);
#pragma warning restore CA2000 // Dispose objects before losing scope

                var storageCredentials = new StorageCredentials(tokenCredential);

                account = new CloudStorageAccount(storageCredentials, _options.Name, string.Empty, true);

                _logger.LogInformation("[Azure Storage][Authentication] Using MSI Token.");
            }
            else if (_options.Name != null
                && !string.IsNullOrEmpty(_options.Token))
            {
                account = new CloudStorageAccount(new StorageCredentials(_options.Token), _options.Name, true);

                _logger.LogInformation("[Azure Storage][Authentication] Using SAS Token.");
            }
            else
            {
                throw new ArgumentException($"One of the following must be set: '{_options.ConnectionString}' or '{_options.Name}'!");
            }

            return account;
        }

        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId DebugInfo = new EventId(100, nameof(DebugInfo));
            }

            private static readonly Action<ILogger, string, string, Exception> _debugInfo = LoggerMessage.Define<string, string>(
                Microsoft.Extensions.Logging.LogLevel.Debug,
                EventIds.DebugInfo,
                "[Azure Storage][{methodName}] {debugInfo}");

            public static void DebugInfo(ILogger logger, string methodName, string debugInfo)
            {
                _debugInfo(logger, methodName, debugInfo, null);
            }
        }
    }
}
