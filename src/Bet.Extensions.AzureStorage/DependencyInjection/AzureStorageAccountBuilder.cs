using System;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class AzureStorageAccountBuilder : IAzureStorageAccountBuilder
    {
        public AzureStorageAccountBuilder(IServiceCollection services, string accountName)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            AccountName = accountName;
        }

        public IServiceCollection Services { get; }

        public string AccountName { get; }
    }
}
