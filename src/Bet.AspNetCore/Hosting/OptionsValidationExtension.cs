using Bet.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.Hosting
{
    public static class OptionsValidationExtension
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            var validator = new OptionsValidationStartupFilter();

            services.AddTransient<IValidationFilter>(_ => validator);
            services.AddSingleton<IStartupFilter>(_ => validator);

            return services;
        }
    }
}
