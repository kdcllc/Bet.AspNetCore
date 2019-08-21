using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.Middleware.Diagnostics
{
    public class DeveloperListRegisteredServicesOptions
    {
        /// <summary>
        /// The default path is "/di".
        /// </summary>
        public string Path { get; set; } = "/di";

        /// <summary>
        /// The default value is json output.
        /// </summary>
        public PathOutputOptions PathOutputOptions { get; set; } = PathOutputOptions.Json;

        internal IReadOnlyList<ServiceDescriptor> Services { get; set; }
    }
}
