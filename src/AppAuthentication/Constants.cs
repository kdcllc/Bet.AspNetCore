namespace AppAuthentication
{
    internal static class Constants
    {
        public const string CLIToolName = "appauthentication";

        // MSI related constants
        public static readonly string MsiAppServiceEndpointEnv = "MSI_ENDPOINT";

        public static readonly string MsiAppServiceSecretEnv = "MSI_SECRET";

        public static readonly string MsiContainerUrl = "host.docker.internal";

        public static readonly string MsiLocalhostUrl = "localhost";

        public static readonly string HostUrl = "http://{0}:{1}/";

        public static readonly string MsiEndpoint = "oauth2/token";

        public static readonly string AzureAuthConnectionStringEnv = "AzureServicesAuthConnectionString";

        public static readonly string MsiRunAsApp = "RunAs=App;";

        // https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication#connection-string-support
        // RunAs=Developer; DeveloperTool=AzureCli
        // RunAs=Developer; DeveloperTool=VisualStudio
    }
}
