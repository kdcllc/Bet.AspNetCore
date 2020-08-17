using System;

namespace Bet.Extensions.AzureAppConfiguration.Options
{
    public class AzureAppConfigConnectOptions
    {
        /// <summary>
        /// The connection string to Azure App Configuration store. If present then it is used.
        /// The default is null.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// The Azure App Configuration store Endpoint URI.
        /// It requires utilization of Microsoft Managed Identity setup.
        /// For local development use AppAuthentication dotnetcore cli tool.
        /// <![CDATA[
        ///    # install
        ///    dotnet tool install --global appauthentication
        ///    # run this first before opening vs.net or vscode; this it will create proper environments for you.
        ///    appauthentication run -l --verbose:debug
        /// ]]>
        /// </summary>
        public Uri? Endpoint { get; set; } = default;

        /// <summary>
        /// HttpClient retries.
        /// </summary>
        public int MaxRetries { get; set; } = 5;
    }
}
