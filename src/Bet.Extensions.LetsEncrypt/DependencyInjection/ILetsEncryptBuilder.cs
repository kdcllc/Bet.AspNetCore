namespace Microsoft.Extensions.DependencyInjection
{
    public interface ILetsEncryptBuilder
    {
        IServiceCollection Services { get; }

        public string Name { get; }
    }
}
