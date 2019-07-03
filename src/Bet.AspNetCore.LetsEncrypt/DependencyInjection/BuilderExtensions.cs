using System.Linq;

using Bet.AspNetCore.LetsEncrypt;
using Bet.AspNetCore.LetsEncrypt.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static ILetsEncryptBuilder AddInMemoryProvider(this ILetsEncryptBuilder builder)
        {
            if (!builder.Services.Any(x => x.ServiceType == typeof(IChallengeStore)))
            {
                builder.Services.AddSingleton<IChallengeStore, DefaultChallengeStore>();
                builder.Services.AddSingleton<IChallengeStoreProvider, InMemoryChallengeStoreProvider>();
            }

            return builder;
        }
    }
}
