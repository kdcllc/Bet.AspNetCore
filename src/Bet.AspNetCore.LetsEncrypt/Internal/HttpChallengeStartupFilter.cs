using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class HttpChallengeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // TODO: figure out how to create dynamic query to get the right builder.
                app.UseLetsEncryptDomainVerification(string.Empty);
                next(app);
            };
        }
    }
}
