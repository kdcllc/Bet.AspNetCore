using System;

using Bet.AspNetCore.Jwt.Options;
using Bet.AspNetCore.Jwt.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtAuthServiceCollectionExtensions
    {
        /// <summary>
        /// Add Jwt Authentication with <see cref="DefaultUserService"/> that always returns true.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            return services
                .AddJwtAuthentication<DefaultUserService>(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                });
        }

        /// <summary>
        /// Adds Jwt Authentication with custom <see cref="IUserService"/> implementation.
        /// </summary>
        /// <typeparam name="TUserService"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAuthOptions"></param>
        /// <param name="configureJwtOptions"></param>
        /// <param name="configureJwtTokenOptions"></param>
        /// <param name="configureUserStoreOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuthentication<TUserService>(
            this IServiceCollection services,
            Action<AuthenticationOptions> configureAuthOptions,
            Action<JwtBearerOptions>? configureJwtOptions = default,
            Action<JwtTokenAuthOptions, IConfiguration>? configureJwtTokenOptions = default,
            Action<UserStoreOptions, IConfiguration>? configureUserStoreOptions = default)
            where TUserService : IUserService
        {
            services.AddChangeTokenOptions<JwtTokenAuthOptions>(
                sectionName: nameof(JwtTokenAuthOptions),
                configureAction: (o, c) => configureJwtTokenOptions?.Invoke(o, c));

            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, PostConfigureJwtBearerOptions>();

            services.AddAuthentication(configureAuthOptions)
                    .AddJwtBearer(options => configureJwtOptions?.Invoke(options));

            services.TryAdd(ServiceDescriptor.Describe(
                        typeof(IUserService),
                        typeof(TUserService),
                        ServiceLifetime.Scoped));

            services.AddScoped<IAuthenticateService, JwtTokenAuthenticationService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IUserStore, InMemoryUserStore>();

            services.AddChangeTokenOptions<UserStoreOptions>(
                nameof(UserStoreOptions),
                nameof(InMemoryUserStore),
                (o, c) => configureUserStoreOptions?.Invoke(o, c));

            services.AddControllers();

            return services;
        }
    }
}
