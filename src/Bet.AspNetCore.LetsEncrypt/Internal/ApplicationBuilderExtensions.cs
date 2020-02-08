using Microsoft.AspNetCore.Builder;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseLetsEncryptDomainVerification(
            this IApplicationBuilder builder,
            string path = "/.well-known/acme-challenge")
        {
            builder.Map(path, app =>
            {
                app.UseMiddleware<ChallengeApprovalMiddleware>();
            });

            return builder;
        }
    }
}
