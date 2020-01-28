using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IAzureStorageAccountBuilder
    {
        IServiceCollection Services { get; }

        string AccountName { get; }
    }
}
