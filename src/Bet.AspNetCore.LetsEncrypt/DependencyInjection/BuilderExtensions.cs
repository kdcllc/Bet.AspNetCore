using System.Linq;

using Bet.AspNetCore.LetsEncrypt;
using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.AspNetCore.LetsEncrypt.InMemory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static ILetsEncryptBuilder AddInMemoryProvider(this ILetsEncryptBuilder builder)
        {
            if (!builder.Services.Any(x => x.ServiceType == typeof(IChallengeStore)))
            {
                builder.Services.AddSingleton<IChallengeStore, ChallengeStore>();
                builder.Services.AddSingleton<IChallengeStoreProvider, InMemoryChallengeStoreProvider>();
            }

            if (!builder.Services.Any(x => x.ServiceType == typeof(ICertificateStore)))
            {
                builder.Services.AddSingleton<ICertificateStore, CertificateStore>();
                builder.Services.AddSingleton<ICertificateStoreProvider, InMemoryCertificateStoreProvider>();
            }

            return builder;
        }
    }
}
