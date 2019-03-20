using System.ComponentModel.DataAnnotations;

namespace Bet.AspNetCore.Options
{
    /// <summary>
    /// Provides a place holder for validation and creation of Azure Vault.
    /// </summary>
    public class AzureVaultOptions
    {
        /// <summary>
        /// Url for Azure Vault 'https://{name}.vault.azure.net/'
        /// </summary>
        [Url]
        public string BaseUrl { get; set; }

        [RegularExpression(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$", ErrorMessage = "Must be valid Guid Id")]
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
