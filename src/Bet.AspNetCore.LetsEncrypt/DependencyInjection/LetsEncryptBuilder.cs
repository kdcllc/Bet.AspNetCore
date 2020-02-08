using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class LetsEncryptBuilder : ILetsEncryptBuilder
    {
        public LetsEncryptBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
