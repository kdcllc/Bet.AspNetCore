using System;
using System.Collections.Generic;
using System.Linq;

using Bet.AspNetCore.Options;
using Bet.Extensions.AzureVault;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;

using Polly;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Known issues with running inside Docker container.
    /// https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/key-vault/service-to-service-authentication.md
    /// https://rahulpnath.com/blog/authenticating-with-azure-key-vault-using-managed-service-identity/
    /// AzureServicesAuthConnectionString=RunAs=App;AppId=AppId;TenantId=TenantId;AppKey=Secret.
    /// </summary>
    public static class AzureVaultKeyBuilder
    {
        internal static readonly Dictionary<string, string> Enviroments = new Dictionary<string, string>
        {
            { "Development", "dev" },
            { "Staging", "qa" },
            { "Production", "prod" }
        };

        /// <summary>
        /// Adds Azure Key Vault with VS.NET authentication in the Development and MSI in production.
        /// If MSI authentication fails it falls back to Client Id and Secret pair if specified in the configuration.
        /// If needed use AppAuthentication global cli tool for Docker MSI authentication.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> builder.</param>
        /// <param name="hostingEnviromentName">The name of the environment retrieved from the Hosting Provider.</param>
        /// <param name="usePrefix">The prefix like dev,qa,prod.</param>
        /// <param name="tokenAuthRetry">The default value for the retry is 2.</param>
        /// <returns></returns>
        public static IConfigurationRoot AddAzureKeyVault(
            this IConfigurationBuilder builder,
            string hostingEnviromentName,
            bool usePrefix = true,
            int tokenAuthRetry = 2)
        {
            var config = builder.Build();
            var options = config.Bind<AzureVaultOptions>("AzureVault");

            var prefix = string.Empty;
            if (usePrefix)
            {
                Enviroments.TryGetValue(hostingEnviromentName, out prefix);
            }

            if (!string.IsNullOrWhiteSpace(options?.BaseUrl))
            {
                try
                {
                    var policy = Policy
                        .Handle<AzureServiceTokenProviderException>()
                        .WaitAndRetry(tokenAuthRetry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();

                    KeyVaultClient Kv() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                    var keyVaultClient = policy.Execute(Kv);

                    // load values that are not specific to the environment.
                    builder.AddAzureKeyVault(options.BaseUrl, keyVaultClient, new PrefixExcludingKeyVaultSecretManager());

                    if (!string.IsNullOrEmpty(prefix))
                    {
                        builder.AddAzureKeyVault(options.BaseUrl, keyVaultClient, new PrefixKeyVaultSecretManager(prefix));
                    }
                    else
                    {
                        builder.AddAzureKeyVault(options.BaseUrl, keyVaultClient, new DefaultKeyVaultSecretManager());
                    }

                    return builder.Build();
                }
                catch (Exception)
                {
                    var list = builder.Sources.ToList();
                    var found = list.Where(x => x.GetType().FullName.Contains("AzureKeyVaultConfigurationSource"));
                    if (found != null)
                    {
                        foreach (var item in found)
                        {
                            builder.Sources.Remove(item);
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(options?.ClientId)
                && !string.IsNullOrWhiteSpace(options?.ClientSecret))
            {
                var secret = options.ClientSecret.FromBase64String();

                // load values that are not specific to the environment.
                builder.AddAzureKeyVault(options.BaseUrl, options.ClientId, secret, new PrefixExcludingKeyVaultSecretManager());

                if (!string.IsNullOrEmpty(prefix))
                {
                    builder.AddAzureKeyVault(options.BaseUrl, options.ClientId, secret, new PrefixKeyVaultSecretManager(prefix));
                }
                else
                {
                    builder.AddAzureKeyVault(options.BaseUrl, options.ClientId, secret, new DefaultKeyVaultSecretManager());
                }
            }

            return builder.Build();
        }

        /// <summary>
        /// Adds Azure Key Vaults with VS.NET or MSI authentication only.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> configuration builder instance.</param>
        /// <param name="keyVaultEndpoints">The default Azure Key Vaults values separated by ';'.</param>
        /// <param name="usePrefix">The default is true. It adds prefixed values from the vault.</param>
        /// <param name="hostingEnviromentName">The hosting environment that is matched to 'dev, stage or prod'.</param>
        /// <returns></returns>
        public static IConfigurationRoot AddAzureKeyVaults(
            this IConfigurationBuilder builder,
            string keyVaultEndpoints,
            bool usePrefix = true,
            string hostingEnviromentName = null)
        {
            if (!string.IsNullOrEmpty(keyVaultEndpoints))
            {
                var prefix = string.Empty;
                if (usePrefix)
                {
                    Enviroments.TryGetValue(hostingEnviromentName, out prefix);
                }

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
#pragma warning disable CA2000 // Dispose objects before losing scope
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
#pragma warning restore CA2000 // Dispose objects before losing scope

                foreach (var splitEndpoint in keyVaultEndpoints.Split(';'))
                    {
                        builder.AddAzureKeyVault(splitEndpoint, keyVaultClient, new PrefixExcludingKeyVaultSecretManager());

                        if (!string.IsNullOrEmpty(prefix))
                        {
                            builder.AddAzureKeyVault(splitEndpoint, keyVaultClient, new PrefixKeyVaultSecretManager(prefix));
                        }
                    }
            }

            return builder.Build();
        }
    }
}
