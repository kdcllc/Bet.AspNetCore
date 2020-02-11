using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Account.Stores;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;
using Bet.Extensions.LetsEncrypt.Order;
using Bet.Extensions.LetsEncrypt.Order.Stores;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AcmeServiceCollectionExtensions
    {
        public static ILetsEncryptBuilder AddLetsEncryptClient(
            this IServiceCollection services,
            string name = "")
        {
            var builder = new LetsEncryptBuilder(services, name);

            // Acme Account generic registrations
            builder.Services.TryAddSingleton<IAcmeContextClientFactory, AcmeContextClientFactory>();

            builder.Services.AddSingleton<IAcmeAccountStore, FileAcmeAccountStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<FileAcmeAccountStoreOptions>>().Get(builder.Name);
                return new FileAcmeAccountStore(Options.Options.Create(options));
            });

            builder.Services.AddSingleton<IAcmeAccountStore, AzureAcmeAccountStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<AzureAcmeAccountStoreOptions>>().Get(builder.Name);
                var storage = sp.GetRequiredService<IStorageBlob<StorageBlobOptions>>();

                return new AzureAcmeAccountStore(Options.Options.Create(options), storage);
            });

            // Acme Order generic registrations
            builder.Services.TryAddSingleton<IAcmeOrderClient, AcmeOrderClient>();

            builder.Services.AddSingleton<IAcmeChallengeStore, InMemoryChallengeStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<ChallengeStoreOptions>>().Get(builder.Name);
                return new InMemoryChallengeStore(Options.Options.Create(options));
            });

            builder.Services.AddSingleton<IAcmeChallengeStore, FileChallengeStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<FileChallengeStoreOptions>>().Get(builder.Name);
                return new FileChallengeStore(Options.Options.Create(options));
            });

            // Certificate related generic registrations
            builder.Services.TryAddSingleton<ICertificateValidator, CertificateValidator>();

            builder.Services.AddSingleton<ICertificateStore, FileCertificateStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<FileCertificateStoreOptions>>().Get(builder.Name);
                return new FileCertificateStore(Options.Options.Create(options));
            });

            builder.Services.AddSingleton<ICertificateStore, AzureCertificateStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<AzureCertificateStoreOptions>>().Get(builder.Name);
                var storage = sp.GetRequiredService<IStorageBlob<StorageBlobOptions>>();

                return new AzureCertificateStore(Options.Options.Create(options), storage);
            });

            return builder;
        }
    }
}
