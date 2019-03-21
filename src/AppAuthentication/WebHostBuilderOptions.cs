namespace AppAuthentication
{
    internal class WebHostBuilderOptions
    {
        /// <summary>
        /// Default set to https://vault.azure.net/
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Azure AD directory or Tenant GUID.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Specify environment.
        /// </summary>
        public string HostingEnviroment { get; set; }

        public bool Verbose { get; set; }

        /// <summary>
        /// Web Host Port number.
        /// </summary>
        public int? Port { get; set; }

        public string ConfigFile { get; set; }

        public string[] Arguments { get; set; }
    }
}
