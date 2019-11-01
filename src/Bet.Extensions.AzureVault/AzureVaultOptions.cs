using System.ComponentModel.DataAnnotations;

using Bet.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bet.AspNetCore.Options
{
    /// <summary>
    /// Provides a place holder for validation and creation of Azure Vault.
    /// </summary>
    public class AzureVaultOptions : IOptionsFormatter
    {
        /// <summary>
        /// Url for Azure Vault 'https://{name}.vault.azure.net/'.
        /// </summary>
        [Url]
        public string BaseUrl { get; set; } = string.Empty;

        [RegularExpression(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$", ErrorMessage = "Must be valid Guid Id")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// The Client Secret must be Base64String.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        public string Format()
        {
            var options = new JObject
            {
                { nameof(BaseUrl), BaseUrl }
            };

            return options.ToString(Formatting.Indented);
        }
    }
}
