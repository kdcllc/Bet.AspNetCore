using System;
using System.IO;

using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Account.Stores;
using Bet.Extensions.LetsEncrypt.AcmeChallenges;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;
using Bet.Extensions.LetsEncrypt.Order;
using Bet.Extensions.LetsEncrypt.Order.Stores;
using DnsClient;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using static System.Environment;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LetsEncryptBuilderExtensions
    {
        public static ILetsEncryptBuilder ConfigureAcmeAccountWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeAccount",
            string? rootPath = null,
            Action<AcmeAccountOptions>? configure = null)
        {
            builder.Services
                    .AddOptions<FileAcmeAccountStoreOptions>(builder.Name)
                    .Configure(options =>
                    {
                        if (string.IsNullOrEmpty(rootPath))
                        {
                            options.RootPath = Path.Combine(
                                GetFolderPath(SpecialFolder.UserProfile, SpecialFolderOption.DoNotVerify),
                                ".acme",
                                "accounts");
                        }
                        else
                        {
                            options.RootPath = rootPath;
                        }
                    });

            builder.Services
                    .AddOptions<AcmeAccountOptions>(builder.Name)
                    .Configure<IConfiguration>((options, configuration) =>
                    {
                        configuration.Bind(section, options);

                        configure?.Invoke(options);
                    })
                    .PostConfigure<IServiceProvider>((options, sp) =>
                    {
                        var storeOptions = sp.GetRequiredService<IOptionsMonitor<FileAcmeAccountStoreOptions>>().Get(builder.Name);
                        options.AccountStore = new FileAcmeAccountStore(storeOptions);
                    });

            builder.Services.TryAddScoped<IAcmeContextClientFactory, AcmeContextClientFactory>();

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureAcmeOrderWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeOrder",
            string? rootPath = null,
            Action<AcmeOrderOptions>? configure = null)
        {
            builder.Services
                .AddOptions<FileChallengeStoreOptions>(builder.Name)
                .Configure(options =>
                {
                    if (string.IsNullOrEmpty(rootPath))
                    {
                        options.RootPath = Path.Combine(
                            GetFolderPath(SpecialFolder.UserProfile, SpecialFolderOption.DoNotVerify),
                            ".acme",
                            "challenges");
                    }
                    else
                    {
                        options.RootPath = rootPath;
                    }
                });

            builder.Services.AddOptions<AcmeOrderOptions>(builder.Name)
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.Bind(section, options);

                    configure?.Invoke(options);
                })
                .PostConfigure<IServiceProvider>((options, sp) =>
                {
                    var storeOptions = sp.GetRequiredService<IOptionsMonitor<FileChallengeStoreOptions>>().Get(builder.Name);

                    options.ChallengesStore = new FileChallengeStore(storeOptions);
                });

            builder.Services.TryAddScoped<IAcmeOrderClient, AcmeOrderClient>();

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureCertificateWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:Certificate",
            string? rootPath = null,
            Action<CertificateOptions>? configure = null)
        {
            builder.Services
                .AddOptions<FileCertificateStoreOptions>(builder.Name)
                .Configure(options =>
                {
                    if (string.IsNullOrEmpty(rootPath))
                    {
                        options.RootPath = Path.Combine(
                            GetFolderPath(SpecialFolder.UserProfile, SpecialFolderOption.DoNotVerify),
                            ".acme");
                    }
                    else
                    {
                        options.RootPath = rootPath;
                    }
                });

            builder.Services
                .AddOptions<CertificateOptions>(builder.Name)
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.Bind(section, options);
                    configure?.Invoke(options);
                });

            builder.Services.TryAddScoped<ICertificateValidator, CertificateValidator>();

            return builder;
        }

        public static ILetsEncryptBuilder AddDnsChallengeResponse(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AzureAuthentication",
            Action<AzureAuthenticationOptions>? configure = null)
        {
            builder.Services.AddScoped((sp) => new LookupClient() { UseCache = false });

            builder.Services.AddScoped<AzureDnsChallenge>();

            builder.Services.AddOptions<AzureAuthenticationOptions>(builder.Name)
                            .Configure<IConfiguration>((options, configuration) =>
                            {
                                configuration.Bind(section, options);
                                configure?.Invoke(options);
                            });

            return builder;
        }
    }
}
