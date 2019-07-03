using System;
using System.Linq;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class ChallengeApprovalMiddleware : IMiddleware
    {
        private readonly IChallengeStore _store;
        private readonly ILogger<ChallengeApprovalMiddleware> _logger;

        public ChallengeApprovalMiddleware(IChallengeStore store, ILogger<ChallengeApprovalMiddleware> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // assumes that this middleware has been mapped
            var requestedToken = context.Request.Path.ToString();
            if (requestedToken.StartsWith("/"))
            {
                requestedToken = requestedToken.Substring(1);
            }

            var allChallenges = await _store.GetChallengesAsync(context.RequestAborted);
            var matchingChallenge = allChallenges.FirstOrDefault(x => x.Token == requestedToken);
            if (matchingChallenge == null)
            {
                _logger.LogInformation(
                    "The given challenge did not match {challengePath} among {allChallenges}",
                    context.Request.Path.ToString(), allChallenges);

                await next(context);
                return;
            }

            _logger.LogInformation("Confirmed challenge request for {token}", requestedToken);

            context.Response.ContentLength = matchingChallenge.Response.Length;
            context.Response.ContentType = "application/octet-stream";
            await context.Response.WriteAsync(matchingChallenge.Response, context.RequestAborted);
        }
    }
}
