using System;

using Bet.Extensions.AzureAppConfiguration;
using Bet.Extensions.AzureAppConfiguration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseAzureAppConfiguration(
            this IHostBuilder builder,
            string sectionName = Sections.AzureAppConfig,
            string? envSectionName = default,
            bool throwExceptionOnStoreNotFound = false,
            Action<AzureAppConfigConnectOptions, FiltersOptions, FeatureFlagOptions>? configureConnect = default,
            Action<AzureAppConfigurationWorkerOptions, IConfiguration>? configureWorker = default)
        {
            return builder
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var currentEnvName = context.HostingEnvironment.EnvironmentName;

                    configBuilder.AddAzureAppConfiguration(
                        currentEnvName,
                        configureConnect,
                        sectionName,
                        throwExceptionOnStoreNotFound,
                        envSectionName);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddChangeTokenOptions<AzureAppConfigurationWorkerOptions>(
                    $"{Sections.AzureAppConfig}:Worker",
                    configureAction: options => configureWorker?.Invoke(options, context.Configuration));

                    services.AddHostedService<AzureAppConfigurationWorker>();
                });
        }
    }
}
