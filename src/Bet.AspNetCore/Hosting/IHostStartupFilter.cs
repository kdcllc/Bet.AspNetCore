using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides options validations on Application startup.
    /// </summary>
    public interface IHostStartupFilter
    {
        /// <summary>
        /// List of types for Options registered.
        /// </summary>
        IList<(Type type, string sectionName)> OptionsTypes { get; }

        /// <summary>
        /// Validates <see cref="IOptions{TOptions}"/> by retrieving value.
        /// </summary>
        /// <param name="provider"></param>
        void Configure(IServiceProvider provider);
    }
}
