using Bet.AspNetCore.LetsEncrypt.Internal;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
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
