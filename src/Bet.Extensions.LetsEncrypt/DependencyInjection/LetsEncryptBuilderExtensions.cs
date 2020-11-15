using System;
using System.IO;
using System.Linq;

using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Account.Stores;
using Bet.Extensions.LetsEncrypt.AcmeChallenges;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;
using Bet.Extensions.LetsEncrypt.Order;
using Bet.Extensions.LetsEncrypt.Order.Stores;

using DnsClient;

using Microsoft.Extensions.Configuration;

using static System.Environment;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LetsEncryptBuilderExtensions
    {
        public static ILetsEncryptBuilder ConfigureAcmeAccountWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeAccount",
            string? rootPath = null,
            Action<AcmeAccountOptions, IServiceProvider>? configure = null)
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

                        options.Configured = true;
                    });

            builder.Services.AddChangeTokenOptions<AcmeAccountOptions>(
                section,
                builder.Name,
                (options, sp) =>
                {
                    configure?.Invoke(options, sp);
                    options.AccountStore = sp.GetServices<IAcmeAccountStore>().First(x => x is FileAcmeAccountStore);
                });

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureAcmeAccountWitAzureStorageStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeAccount",
            string blobSection = "AcmeAccount",
            string blobRootSection = "LetsEncrypt:StorageBlobs",
            Action<AcmeAccountOptions, IServiceProvider>? configure = null,
            Action<StorageBlobOptions>? storageConfigure = null)
        {
            builder.Services
                  .AddAzureStorageAccount("letsencrypt")
                  .AddAzureBlobContainer($"{builder.Name}-account", blobSection, blobRootSection, storageConfigure);

            builder.Services
                    .AddOptions<AzureAcmeAccountStoreOptions>(builder.Name)
                    .Configure(options =>
                    {
                        options.NamedOption = builder.Name;
                        options.Configured = true;
                    });

            builder.Services.AddChangeTokenOptions<AcmeAccountOptions>(
                section,
                builder.Name,
                (options, sp) =>
                {
                    configure?.Invoke(options, sp);
                    options.AccountStore = sp.GetServices<IAcmeAccountStore>().First(x => x is AzureAcmeAccountStore);
                });

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureAcmeOrderWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeOrder",
            string? rootPath = null,
            Action<AcmeOrderOptions, IServiceProvider>? configure = null)
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

                    options.Configured = true;
                });

            builder.Services.AddChangeTokenOptions<AcmeOrderOptions>(
                section,
                builder.Name,
                (options, sp) =>
                {
                    configure?.Invoke(options, sp);
                    options.ChallengesStore = sp.GetServices<IAcmeChallengeStore>().First(x => x is FileChallengeStore);
                });

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureAcmeOrderWithInMemoryStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AcmeOrder",
            Action<AcmeOrderOptions, IServiceProvider>? configure = null)
        {
            builder.Services
               .AddOptions<ChallengeStoreOptions>(builder.Name)
               .Configure(options =>
               {
                   options.Configured = true;
               });

            builder.Services.AddChangeTokenOptions<AcmeOrderOptions>(
                section,
                builder.Name,
                (options, sp) =>
                {
                    configure?.Invoke(options, sp);
                    options.ChallengesStore = sp.GetServices<IAcmeChallengeStore>().First(x => x is InMemoryChallengeStore);
                });

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureCertificateWithAzureStorageStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:Certificate",
            string blobSection = "Certificates",
            string blobRootSection = "LetsEncrypt:StorageBlobs",
            Action<CertificateOptions, IServiceProvider>? configure = null,
            Action<StorageBlobOptions>? storageConfigure = null)
        {
            builder.Services
                  .AddAzureStorageAccount("letsencrypt")
                  .AddAzureBlobContainer($"{builder.Name}-cert", blobSection, blobRootSection, storageConfigure);

            builder.Services
                .AddOptions<AzureCertificateStoreOptions>(builder.Name)
                .Configure(options =>
                {
                    options.NamedOption = builder.Name;
                    options.Configured = true;
                });

            builder.Services.AddChangeTokenOptions(section, builder.Name, configure);

            return builder;
        }

        public static ILetsEncryptBuilder ConfigureCertificateWithFileStore(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:Certificate",
            string? rootPath = null,
            Action<CertificateOptions, IServiceProvider>? configure = null)
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

                    options.NamedOption = builder.Name;
                    options.Configured = true;
                });

            builder.Services.AddChangeTokenOptions(section, builder.Name, configure);

            return builder;
        }

        public static ILetsEncryptBuilder AddDnsChallengeResponse(
            this ILetsEncryptBuilder builder,
            string section = "LetsEncrypt:AzureAuthentication",
            Action<AzureAuthenticationOptions>? configure = null)
        {
            builder.Services.AddScoped((sp) => new LookupClient(new LookupClientOptions { UseCache = false }));

            builder.Services.AddScoped<AzureDnsChallenge>();

            builder.Services
                    .AddOptions<AzureAuthenticationOptions>(builder.Name)
                    .Configure<IConfiguration>((options, configuration) =>
                    {
                        configuration.Bind(section, options);
                        configure?.Invoke(options);
                    });

            return builder;
        }
    }
}
