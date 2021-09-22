using System;

using Bet.AspNetCore.ApiKeyAuthentication;
using Bet.AspNetCore.ApiKeyAuthentication.Options;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiKeyAuthenticationServiceExtentions
    {
        /// <summary>
        /// Adds Web Api Query Authetnication.
        /// </summary>
        /// <typeparam name="TApiUserStore"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="configApiUserStoreOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiKeyQueryAuthentication<TApiUserStore>(
            this IServiceCollection services,
            Action<ApiKeyAuthenticationOptions> configure,
            Action<ApiUserStoreOptions, IConfiguration>? configApiUserStoreOptions = default)
            where TApiUserStore : IApiUserStore
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            }).AddApiKeyQuerySupport(configure);

            services.AddChangeTokenOptions<ApiUserStoreOptions>(
                nameof(ApiUserStoreOptions),
                nameof(InMemoryApiUserStore),
                (o, c) => configApiUserStoreOptions?.Invoke(o, c));

            services.TryAdd(ServiceDescriptor.Describe(
                        typeof(IApiUserStore),
                        typeof(TApiUserStore),
                        ServiceLifetime.Scoped));

            return services;
        }

        /// <summary>
        /// Adds Web Api Header Authetnication.
        /// </summary>
        /// <typeparam name="TApiUserStore"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="configApiUserStoreOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiKeyHeaderAuthentication<TApiUserStore>(
            this IServiceCollection services,
            Action<ApiKeyAuthenticationOptions> configure,
            Action<ApiUserStoreOptions, IConfiguration>? configApiUserStoreOptions = default)
            where TApiUserStore : IApiUserStore
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            }).AddApiKeyHeaderSupport(configure);

            services.AddChangeTokenOptions<ApiUserStoreOptions>(
                nameof(ApiUserStoreOptions),
                nameof(InMemoryApiUserStore),
                (o, c) => configApiUserStoreOptions?.Invoke(o, c));

            services.TryAdd(ServiceDescriptor.Describe(
                        typeof(IApiUserStore),
                        typeof(TApiUserStore),
                        ServiceLifetime.Scoped));

            return services;
        }
    }
}
