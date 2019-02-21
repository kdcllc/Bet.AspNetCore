using Bet.AspNetCore.Options;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Known issues with running inside Docker container.
    /// https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/key-vault/service-to-service-authentication.md
    /// https://rahulpnath.com/blog/authenticating-with-azure-key-vault-using-managed-service-identity/
    /// AzureServicesAuthConnectionString=RunAs=App;AppId=AppId;TenantId=TenantId;AppKey=Secret
    /// </summary>
    public static class AzureVaultKeyBuilder
    {
        private static readonly Dictionary<string, string> _enviroments = new Dictionary<string, string>
        {
            {"Development", "dev" },
            {"Staging", "qa" },
            {"Production", "prod" }
        };

        /// <summary>
        /// Add Azure Key Vaults with VS.NET authentication or thru AppId=SecretID pair.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> builder.</param>
        /// <param name="hostingEnviromentName">The name of the environment retrieved from the Hosting Provider.</param>
        /// <param name="usePrefix">The prefix like dev,qa,prod.</param>
        /// <returns></returns>
        public static IConfigurationRoot AddAzureKeyVault(
            this IConfigurationBuilder builder,
            string hostingEnviromentName,
            bool usePrefix = true)
        {
            var config = builder.Build();
            var options = config.Bind<AzureVaultOptions>("AzureVault");

            var prefix = string.Empty;
            if (usePrefix)
            {
                _enviroments.TryGetValue(hostingEnviromentName, out prefix);
            }

            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                try
                {
                    var policy = Policy
                        .Handle<AzureServiceTokenProviderException>()
                        .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();

                    KeyVaultClient kv() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                    var keyVaultClient = policy.Execute(kv);

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
                catch (AzureServiceTokenProviderException)
                {
                    var list = builder.Sources.ToList();
                    var found = list.FirstOrDefault(x => x.GetType().FullName.Contains("AzureKeyVaultConfigurationSource"));
                    if (found != null)
                    {
                        builder.Sources.Remove(found);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(options.ClientId)
                && !string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                var secretBytes = Convert.FromBase64String(options.ClientSecret);
                var secret = System.Text.Encoding.ASCII.GetString(secretBytes);

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
    }
}
