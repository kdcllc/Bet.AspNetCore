using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class ChallengeApprovalStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseLetsEncryptDomainVerification();
                next(app);
            };
        }
    }
}
