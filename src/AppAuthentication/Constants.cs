namespace AppAuthentication
{
    public static class Constants
    {
        public const string CLIToolName = "appauthentication";

        // MSI related constants
        public static readonly string MsiAppServiceEndpointEnv = "MSI_ENDPOINT";

        public static readonly string MsiAppServiceSecretEnv = "MSI_SECRET";

        public static readonly string MsiContinerUrl = "host.docker.internal";

        public static readonly string HostUrl = "http://{0}:{1}/";

        public static readonly string MsiEndpoint = "oauth2/token";
    }
}
