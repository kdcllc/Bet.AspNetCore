using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseLetsEncryptDomainVerification(
            this IApplicationBuilder builder,
            string named)
        {
            var options = builder.ApplicationServices.GetRequiredService<IOptionsMonitor<HttpChallengeResponseOptions>>().Get(named);
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<HttpChallengeResponseMiddleware>>();
            builder.Map(options.ValidationPath, app =>
            {
                app.UseMiddleware<HttpChallengeResponseMiddleware>(options.ChallengeStore, logger);
            });

            return builder;
        }
    }
}
