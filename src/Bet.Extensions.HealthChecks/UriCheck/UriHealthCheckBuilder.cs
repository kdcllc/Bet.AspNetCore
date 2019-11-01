using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.HealthChecks.UriCheck
{
    public class UriHealthCheckBuilder : IUriHealthCheckBuilder
    {
        public UriHealthCheckBuilder(
            IServiceCollection services,
            string checkName)
        {
            Services = services;
            CheckName = checkName;
        }

        public IServiceCollection Services { get; }

        public string CheckName { get; }

        public IUriHealthCheckBuilder Add(Action<UriOptionsSetup> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var option = new UriOptionsSetup();

            action(option);

            Services.Configure<UriHealthCheckOptions>(CheckName, options =>
            {
                options.UriOptions.Add(option);
            });

            return this;
        }

        public IUriHealthCheckBuilder Add(UriOptionsSetup optionsSetup)
        {
            if (optionsSetup == null)
            {
                throw new ArgumentNullException(nameof(optionsSetup));
            }

            Services.Configure<UriHealthCheckOptions>(CheckName, option =>
            {
                option.UriOptions.Add(optionsSetup);
            });
            return this;
        }
    }
}
