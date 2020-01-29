using System;

using Bet.Extensions.DataProtection.AzureStorage;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataProtectionServiceCollectionExtensions
    {
        public static IServiceCollection AddDataProtectionAzureStorage(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "AzureDataProtection",
            string dataProtectionSectionName = "DataProtectionAzureStorage",
            Action<DataProtectionAzureStorageOptions>? setup = null)
        {
            var enabledDataProtection = configuration.GetValue<bool>(sectionName);
            if (enabledDataProtection)
            {
                services.AddDataProtectionAzureStorage(dataProtectionSectionName, setup);
            }

            return services;
        }

        public static IServiceCollection AddDataProtectionAzureStorage(
            this IServiceCollection services,
            string dataProtectionSectionName = "DataProtectionAzureStorage",
            Action<DataProtectionAzureStorageOptions>? setup = null)
        {
            services.AddDataProtectionOptions(dataProtectionSectionName, setup);

            services.ConfigureOptions<KeyManagementOptionsSetup>();

            services.AddDataProtection().ProtectKeysWithAzureKeyVault();

            return services;
        }

        public static IDataProtectionBuilder ProtectKeysWithAzureKeyVault(this IDataProtectionBuilder builder)
        {
            // not the preferred action here
            var provider = builder.Services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<DataProtectionAzureStorageOptions>>();

#pragma warning disable CA2000 // Dispose objects before losing scope
            var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(options.Value.TokenProvider.KeyVaultTokenCallback));
#pragma warning restore CA2000 // Dispose objects before losing scope
            builder.ProtectKeysWithAzureKeyVault(kvClient, options.Value.KeyVaultKeyId);

            return builder;
        }

        private static IServiceCollection AddDataProtectionOptions(
            this IServiceCollection services,
            string dataProtectionSectionName,
            Action<DataProtectionAzureStorageOptions>? setup = null)
        {
            var options = new DataProtectionAzureStorageOptions();
            setup?.Invoke(options);

            services.AddTransient<IConfigureOptions<DataProtectionAzureStorageOptions>>(sp =>
            {
                return new ConfigureOptions<DataProtectionAzureStorageOptions>(options =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(dataProtectionSectionName, options);

                    setup?.Invoke(options);
                });
            });

            // configure changeable configurations
            services.AddSingleton((Func<IServiceProvider, IOptionsChangeTokenSource<DataProtectionAzureStorageOptions>>)((sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>().GetSection(dataProtectionSectionName);
                return new ConfigurationChangeTokenSource<DataProtectionAzureStorageOptions>(config);
            }));

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<DataProtectionAzureStorageOptions>>().Value);

            services.AddSingleton((Func<IServiceProvider, IConfigureOptions<DataProtectionAzureStorageOptions>>)(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>().GetSection(dataProtectionSectionName);
                return new ConfigureFromConfigurationOptions<DataProtectionAzureStorageOptions>(config);
            }));

            services.Configure<DataProtectionAzureStorageOptions>(opt => setup?.Invoke(opt));

            return services;
        }
    }
}
