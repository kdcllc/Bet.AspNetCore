using Bet.AnalyticsEngine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AnalyticsEngineServiceCollectionExtensions
    {
        public static IServiceCollection AnalyticsEngine(this IServiceCollection services)
        {
            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<AnalyticsRequestContext>();

            services.AddChangeTokenOptions<AnalyticsEngineOptions>(
                nameof(AnalyticsEngineOptions),
                configureAction: (options, config) =>
                {
                    // corresponds to helm value for azurefileshare
                    options.DatabasePath = config["DatabasePath"];
                });

            return services;
        }
    }
}
