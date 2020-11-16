using System;

using Azure.Identity;

using Bet.Extensions;
using Bet.Extensions.AzureAppConfiguration;
using Bet.Extensions.AzureAppConfiguration.Options;

using Microsoft.Extensions.Configuration.AzureAppConfiguration.FeatureManagement;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureAppConfiguration(
            this IConfigurationBuilder configurationBuilder,
            string currentEnvironment,
            Action<AzureAppConfigConnectOptions, FiltersOptions, FeatureFlagOptions>? configureConnect = default,
            string sectionName = Sections.AzureAppConfig,
            bool throwExceptionOnStoreNotFound = false,
            string? envSectionName = null)
        {
            var configuration = configurationBuilder.Build();

            return configurationBuilder.AddAzureAppConfiguration(
                options =>
                {
                    var env = new Environments();
                    if (!string.IsNullOrEmpty(envSectionName))
                    {
                        env.Clear();
                        configuration.Bind(envSectionName, env);
                    }

                    // create connection options
                    var connectOptions = new AzureAppConfigConnectOptions();
                    configuration.Bind(sectionName, connectOptions);

                    var filtersOptions = new FiltersOptions();
                    configuration.Bind($"{sectionName}:Filters", filtersOptions);

                    var featuresOptions = new FeatureFlagOptions();
                    configuration.Bind($"{sectionName}:Features", featuresOptions);

                    configureConnect?.Invoke(connectOptions, filtersOptions, featuresOptions);

                    // configure features
                    options.UseFeatureFlags(fo =>
                    {
                        fo.CacheExpirationInterval = featuresOptions.CacheExpirationInterval;
                        if (!string.IsNullOrEmpty(featuresOptions.Label))
                        {
                            fo.Label = env[featuresOptions.Label];
                        }
                    });

                    if (!string.IsNullOrEmpty(connectOptions.ConnectionString))
                    {
                        options.Connect(connectOptions.ConnectionString);
                    }
                    else if (connectOptions.Endpoint != null
                             && string.IsNullOrEmpty(connectOptions.ConnectionString))
                    {
                        var credentials = new DefaultAzureCredential();
                        options.Connect(connectOptions.Endpoint, credentials);
                    }

                    options.ConfigureClientOptions(clientOptions => clientOptions.Retry.MaxRetries = connectOptions.MaxRetries);

                    foreach (var section in filtersOptions.Sections)
                    {
                        // Load configuration values with no label, which means all of the configurations that are not specific to
                        // Environment
                        options.Select(section);

                        // Override with any configuration values specific to current hosting env
                        options.Select(section, env[currentEnvironment]);
                    }

                    foreach (var section in filtersOptions.RefresSections)
                    {
                        options.ConfigureRefresh(refresh =>
                        {
                            refresh.Register(section, refreshAll: true);
                            refresh.Register(section, env[currentEnvironment], refreshAll: true);
                            refresh.SetCacheExpiration(filtersOptions.CacheExpirationTime);
                        });
                    }
                },
                throwExceptionOnStoreNotFound);
        }
    }
}
