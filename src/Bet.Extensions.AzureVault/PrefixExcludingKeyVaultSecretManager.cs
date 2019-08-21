using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Bet.Extensions.AzureVault
{
    /// <summary>
    /// Manager for loading all keys that are not prefixed with an environment.
    /// </summary>
    public class PrefixExcludingKeyVaultSecretManager : IKeyVaultSecretManager
    {
        public bool Load(SecretItem secret)
        {
            // Load a vault secret when its secret name starts with the
            // prefix. Other secrets won't be loaded.
            var secretName = secret.Identifier.Name;

            var envIndex = secretName.IndexOf("--");

            if (envIndex > -1)
            {
                var env = secretName.Substring(0, envIndex);

                return !AzureVaultKeyBuilder.Enviroments.ContainsValue(env);
            }

            return true;
        }

        public string GetKey(SecretBundle secret)
        {
            // Remove the prefix from the secret name and replace two
            // dashes in any name with the KeyDelimiter, which is the
            // delimiter used in configuration (usually a colon). Azure
            // Key Vault doesn't allow a colon in secret names.
            return secret.SecretIdentifier.Name
                .Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
