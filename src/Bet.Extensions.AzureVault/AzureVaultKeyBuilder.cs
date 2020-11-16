using System;
using System.Collections.Generic;
using System.Linq;

using Bet.AspNetCore.Options;
using Bet.Extensions;
using Bet.Extensions.AzureVault;

using Microsoft.Azure.KeyVault;
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
        /// <summary>
        /// Adds Azure Key Vault with VS.NET authentication in the Development and MSI in production.
        /// If MSI authentication fails it falls back to Client Id and Secret pair if specified in the configuration.
        /// If needed use AppAuthentication global cli tool for Docker MSI authentication.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> builder.</param>
        /// <param name="hostingEnviromentName">The name of the environment retrieved from the Hosting Provider.</param>
        /// <param name="usePrefix">The prefix like dev,qa,prod.</param>
        /// <param name="tokenAuthRetry">The default value for the retry is 2.</param>
        /// <param name="sectionName">The name of the Azure Key Vault Configuration Section. The default is 'AzureVault'.</param>
        /// <param name="reloadInterval">The reload interval for the Azure Key Vault.</param>
        /// <param name="enviroments">The conversion for HostEnvironment:Prefix. The default is null.</param>
        /// <returns></returns>
        public static IConfigurationRoot AddAzureKeyVault(
            this IConfigurationBuilder builder,
            string hostingEnviromentName,
            bool usePrefix = true,
            int tokenAuthRetry = 2,
            string sectionName = "AzureVault",
            TimeSpan? reloadInterval = null,
            Environments? enviroments = null)
        {
            var config = builder.Build();
            var options = config.Bind<AzureVaultOptions>(sectionName);
            enviroments ??= new Environments();

            var prefix = string.Empty;
            if (usePrefix)
            {
                enviroments.TryGetValue(hostingEnviromentName, out prefix);
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
                    builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl)
                    {
                        Client = keyVaultClient,
                        Manager = new PrefixExcludingKeyVaultSecretManager(enviroments),
                        ReloadInterval = reloadInterval
                    });

                    if (!string.IsNullOrEmpty(prefix))
                    {
                        builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl)
                        {
                            Client = keyVaultClient,
                            Manager = new PrefixKeyVaultSecretManager(prefix),
                            ReloadInterval = reloadInterval,
                        });
                    }
                    else
                    {
                        builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl)
                        {
                            Client = keyVaultClient,
                            Manager = new DefaultKeyVaultSecretManager(),
                            ReloadInterval = reloadInterval,
                        });
                    }

                    return builder.Build();
                }
                catch
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
                var secret = options?.ClientSecret.FromBase64String();

                // load values that are not specific to the environment.
                builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl, options?.ClientId, secret)
                {
                    Manager = new PrefixExcludingKeyVaultSecretManager(enviroments),
                    ReloadInterval = reloadInterval
                });

                if (!string.IsNullOrEmpty(prefix))
                {
                    builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl, options?.ClientId, secret)
                    {
                        Manager = new PrefixKeyVaultSecretManager(prefix),
                        ReloadInterval = reloadInterval
                    });
                }
                else
                {
                    builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(options?.BaseUrl, options?.ClientId, secret)
                    {
                        Manager = new DefaultKeyVaultSecretManager(),
                        ReloadInterval = reloadInterval
                    });
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
        /// <param name="reloadInterval">The reload interval for the Azure Key Vault.</param>
        /// <param name="enviroments">The conversion for HostEnvironment:Prefix. The default is null.</param>
        /// <returns></returns>
        public static IConfigurationRoot AddAzureKeyVaults(
            this IConfigurationBuilder builder,
            string keyVaultEndpoints,
            bool usePrefix = true,
            string? hostingEnviromentName = null,
            TimeSpan? reloadInterval = null,
            Environments? enviroments = null)
        {
            if (!string.IsNullOrEmpty(keyVaultEndpoints))
            {
                enviroments ??= new Environments();

                var prefix = string.Empty;
                if (usePrefix && hostingEnviromentName != null)
                {
                    enviroments.TryGetValue(hostingEnviromentName, out prefix);
                }

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
#pragma warning disable CA2000 // Dispose objects before losing scope
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
#pragma warning restore CA2000 // Dispose objects before losing scope

                foreach (var splitEndpoint in keyVaultEndpoints.Split(';'))
                {
                    builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(splitEndpoint)
                    {
                        Client = keyVaultClient,
                        Manager = new PrefixExcludingKeyVaultSecretManager(enviroments),
                        ReloadInterval = reloadInterval
                    });

                    if (!string.IsNullOrEmpty(prefix))
                    {
                        builder.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions(splitEndpoint)
                        {
                            Client = keyVaultClient,
                            Manager = new PrefixKeyVaultSecretManager(prefix),
                            ReloadInterval = reloadInterval
                        });
                    }
                }
            }

            return builder.Build();
        }
    }
}
