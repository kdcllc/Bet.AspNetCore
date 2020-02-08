namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServiceCollectionExtensions
    {
        public static ILetsEncryptBuilder AddLetsEncryptClient(
            this IServiceCollection services,
            string name = "")
        {
            var builder = new LetsEncryptBuilder(services, name);
            return builder;
        }
    }
}
