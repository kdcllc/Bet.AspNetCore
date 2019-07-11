using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultLetsEncryptBuilder : ILetsEncryptBuilder
    {
        public IServiceCollection Services { get; }

        public DefaultLetsEncryptBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
