using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultLetsEncryptBuilder : ILetsEncryptBuilder
    {
        public DefaultLetsEncryptBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
