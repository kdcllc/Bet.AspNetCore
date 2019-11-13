using System.Linq;

using Bet.AspNetCore.Hosting;
using Bet.Extensions.Options;

using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsValidationExtension
    {
        /// <summary>
        /// Adds Options validations on Application Start.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfigurationValidation(this IServiceCollection services)
        {
            var webhostFilter = services.Select(x => x.ImplementationInstance as IValidationFilter).OfType<IValidationFilter>().FirstOrDefault();
            if (webhostFilter == null)
            {
                webhostFilter = new OptionsValidationStartupFilter();

                services.AddSingleton<IValidationFilter>(webhostFilter);
                services.AddSingleton<IStartupFilter>((IStartupFilter)webhostFilter);
            }

            return services;
        }
    }
}
