using System;

using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.AspNetCore.LetsEncrypt.Azure;
using Bet.AspNetCore.LetsEncrypt.InMemory;
using Bet.AspNetCore.LetsEncrypt.Internal;
using Bet.AspNetCore.LetsEncrypt.Options;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.Hosting.Abstractions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LetsEncryptBuilderExtensions
    {
        public static ILetsEncryptBuilder AddLetsEncrypt(
            this IServiceCollection services,
            string sectionName = "LetsEncrypt",
            Action<LetsEncryptOptions, IConfiguration> configure = null,
            TimeSpan? interval = null)
        {
            var builder = new LetsEncryptBuilder(services);

            builder.Services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();

            builder.Services
                .AddSingleton<IStartupFilter, ChallengeApprovalStartupFilter>()
                .AddSingleton<CertificateSelector>()
                .AddSingleton<ChallengeApprovalMiddleware>()
                .AddSingleton<ILetsEncryptService, LetsEncryptService>();

            builder.Services.AddTimedHostedService<CertificateRenewalService>(options =>
                {
                    options.Interval = interval ?? TimeSpan.FromSeconds(30);

                    options.FailMode = FailMode.LogAndRetry;
                    options.RetryInterval = TimeSpan.FromSeconds(2);
                });

            builder.Services
                   .AddChangeTokenOptions(sectionName, null, configure);

            builder.Services.TryAddSingleton<IChallengeStore, ChallengeStore>();
            builder.Services.AddSingleton<IChallengeStoreProvider, InMemoryChallengeStoreProvider>();

            builder.Services.TryAddSingleton<ICertificateStore, CertificateStore>();
            builder.Services.AddSingleton<ICertificateStoreProvider, InMemoryCertificateStoreProvider>();

            return builder;
        }

        public static ILetsEncryptBuilder AddAzureStorage(
            this ILetsEncryptBuilder builder,
            string challangeStore,
            string certificateStore)
        {
            builder.Services
                   .AddAzureStorageAccount("letsencrypt")
                   .AddAzureBlobContainer<StorageBlobOptions>(challangeStore, challangeStore)
                   .AddAzureBlobContainer<StorageBlobOptions>(certificateStore, certificateStore);

            builder.Services.AddSingleton<IChallengeStoreProvider, AzureStorageChallengeStoreProvider>();
            builder.Services.AddSingleton<ICertificateStoreProvider, AzureStorageCertificateStoreProvider>();

            builder.Services.Configure<LetsEncryptOptions>(options =>
            {
                options.ChallengeConainterName = challangeStore;
                options.CertificateContainerName = certificateStore;
            });

            return builder;
        }
    }
}
