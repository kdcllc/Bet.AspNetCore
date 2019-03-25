using Microsoft.Azure.Services.AppAuthentication;

namespace AppAuthentication
{
    /// <summary>
    /// Base class from which other token providers derive.
    /// </summary>
    internal abstract class NonInteractiveAzureServiceTokenProviderBase
    {
        public string ConnectionString;
        public Principal PrincipalUsed;
    }
}
