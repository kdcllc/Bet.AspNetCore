using Microsoft.Extensions.Logging;

namespace AppAuthentication
{
    internal class WebHostBuilderOptions
    {
        /// <summary>
        /// Default set to https://vault.azure.net/.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Azure AD directory or Tenant GUID.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Specify Environment.
        /// </summary>
        public string HostingEnvironment { get; set; }

        /// <summary>
        /// Enables custom <see cref="LogLevel"/>.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// The <see cref="LogLevel"/> for the <see cref="Verbose"/>.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Web Host Port number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Configuration file with settings.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Console apps pass-thru arguments to the generic host.
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        /// Guid Generated Id for the secret.
        /// </summary>
        public string SecretId { get; set; }

        /// <summary>
        /// The default MSI_ENPOINT environment variable supports Docker only callbacks.
        /// </summary>
        public bool IsLocal { get; set; }
    }
}
