namespace AppAuthentication
{
    public static class Constants
    {
        // MSI related constants
        public static readonly string MsiAppServiceEndpointEnv = "MSI_ENDPOINT";
        public static readonly string MsiAppServiceSecretEnv = "MSI_SECRET";
        public static readonly string MsiEndpoint = "http://localhost:{0}/oauth2/token";
    }
}
