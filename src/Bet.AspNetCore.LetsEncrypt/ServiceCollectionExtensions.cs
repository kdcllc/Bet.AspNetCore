using System;

using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.AspNetCore.LetsEncrypt.Internal;
using Bet.AspNetCore.LetsEncrypt.Options;
using Bet.Extensions.Hosting.Abstractions;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ILetsEncryptBuilder AddLetsEncrypt(
            this IServiceCollection services,
            Action<LetsEncryptOptions> configure = null)
        {
            var builder = new DefaultLetsEncryptBuilder(services);

            builder.Services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();

            builder.Services
                .AddSingleton<CertificateSelector>()
                .AddSingleton<ChallengeApprovalMiddleware>()
                .AddSingleton<ILetsEncryptService, LetsEncryptService>()
                .AddTimedHostedService<CertificateRenewalService>(options =>
                {
                    options.Interval = TimeSpan.FromSeconds(30);

                    options.FailMode = FailMode.LogAndRetry;
                    options.RetryInterval = TimeSpan.FromSeconds(2);
                })
                .Configure(configure);

            return builder;
        }
    }
}
