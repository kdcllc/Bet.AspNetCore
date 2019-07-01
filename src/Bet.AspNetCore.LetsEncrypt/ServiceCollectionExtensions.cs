using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ILetsEncryptBuilder AddLetsEncrypt(this IServiceCollection services)
        {
            return new DefaultLetsEncryptBuilder(services);
        }
    }
}
