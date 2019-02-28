using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using OptionsValidationException = Bet.AspNetCore.Options.OptionsValidationException;

namespace Microsoft.AspNetCore.Hosting
{
    public class OptionsValidationStartupFilter : IStartupFilter, IHostStartupFilter
    {
        private IList<(Type type, string sectionName)> _optionsTypes;

        /// <summary>
        /// The type and configuration name of the options to validate.
        /// </summary>
        public IList<(Type type, string sectionName)> OptionsTypes => _optionsTypes ?? (_optionsTypes = new List<(Type type, string sectionName)>());

        /// <inheritdoc/>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                if (_optionsTypes != null)
                {
                    foreach (var (type, sectionName) in _optionsTypes)
                    {
                        var options = app.ApplicationServices.GetService(typeof(IOptions<>).MakeGenericType(type));
                        try
                        {
                            if (options != null)
                            {
                                // Retrieve the value to trigger validation
                                var optionsValue = ((IOptions<object>)options).Value;
                            }
                        }
                        catch (Extensions.Options.OptionsValidationException ex)
                        {
                            throw new OptionsValidationException(ex.Failures, (type, sectionName));
                        }
                    }
                }

                next(app);
            };
        }

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
