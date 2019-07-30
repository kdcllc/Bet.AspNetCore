using System;

using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides options validations on Application startup.
    /// </summary>
    public interface IHostStartupFilter
    {
        /// <summary>
        /// Validates <see cref="IOptions{TOptions}"/> by retrieving value.
        /// </summary>
        /// <param name="provider"></param>
        void Configure(IServiceProvider provider);
    }
}
