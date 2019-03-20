using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using OptionsValidationException = Bet.AspNetCore.Options.OptionsValidationException;

namespace Microsoft.Extensions.Hosting
{
    /// <inheritdoc/>
    public class OptionsValidationHostStartupFilter : IHostStartupFilter
    {
        private IList<(Type type, string sectionName)> _optionsTypes;

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
                    catch (Extensions.Options.OptionsValidationException ex)
                    {
                        throw new OptionsValidationException(ex.Failures, (type, sectionName));
                    }
                }
            }
        }
    }
}
