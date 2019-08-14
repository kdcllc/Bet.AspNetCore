using System;

using Bet.AspNetCore.LetsEncrypt.Internal;
using Bet.AspNetCore.LetsEncrypt.Options;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Use Let's Encrypt (<see href="https://letsencrpyt.org">https://letsencrpyt.org</see>)
        /// to automatically generate HTTPs certificates for this server.
        /// </summary>
        /// <param name="hostBuilder">The web host builder.</param>
        /// <param name="configure">Options for configuring certificate generation.</param>
        /// <returns>IWebHostBuilder.</returns>
        public static IWebHostBuilder UseLetsEncrypt(
            this IWebHostBuilder hostBuilder,
            Action<LetsEncryptOptions> configure)
        {
            hostBuilder.ConfigureServices(services =>
            {
                var builder = services.AddLetsEncrypt(configure);
                builder.AddInMemoryProvider();
                builder.Services.AddSingleton<IStartupFilter, ChallengeApprovalStartupFilter>();
            });

            return hostBuilder;
        }
    }
}
