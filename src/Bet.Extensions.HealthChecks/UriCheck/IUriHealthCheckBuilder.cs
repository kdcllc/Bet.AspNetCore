using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.HealthChecks.UriCheck
{
    public interface IUriHealthCheckBuilder
    {
        /// <summary>
        /// Name of the Uris Check Group.
        /// </summary>
        string CheckName { get; }

        IServiceCollection Services { get; }

        IUriHealthCheckBuilder Add(Action<UriOptionsSetup> action);

        IUriHealthCheckBuilder Add(UriOptionsSetup optionsSetup);
    }
}
