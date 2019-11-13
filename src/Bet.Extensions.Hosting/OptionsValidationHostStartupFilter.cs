using System;
using System.Collections.Generic;

using Bet.Extensions.Options;

using Microsoft.Extensions.Options;

using OptionsValidationException = Bet.Extensions.Options.OptionsValidationException;

namespace Microsoft.Extensions.Hosting
{
    /// <inheritdoc/>
    public class OptionsValidationHostStartupFilter : IHostStartupFilter, IValidationFilter
    {
        private IList<(Type type, string sectionName)>? _optionsTypes;

        /// <inheritdoc/>
        public IList<(Type type, string sectionName)> OptionsTypes => _optionsTypes ?? (_optionsTypes = new List<(Type type, string sectionName)>());

        /// <inheritdoc/>
        public void Configure(IServiceProvider provider)
        {
            if (_optionsTypes != null)
            {
                foreach (var (type, sectionName) in _optionsTypes)
                {
                    var options = provider.GetService(typeof(IOptions<>).MakeGenericType(type));
                    try
                    {
                        if (options != null)
                        {
                            // Retrieve the value to trigger the Asp.Net Core validation
                            var optionsValue = ((IOptions<object>)options).Value;
                        }
                    }
                    catch (Options.OptionsValidationException ex)
                    {
                        throw new OptionsValidationException(ex.Failures, (type, sectionName));
                    }
                }
            }
        }
    }
}
