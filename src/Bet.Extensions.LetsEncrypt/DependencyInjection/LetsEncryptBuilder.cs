using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class LetsEncryptBuilder : ILetsEncryptBuilder
    {
        public LetsEncryptBuilder(IServiceCollection services, string name)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Name = name;
        }

        public IServiceCollection Services { get; }

        public string Name { get; }
    }
}
