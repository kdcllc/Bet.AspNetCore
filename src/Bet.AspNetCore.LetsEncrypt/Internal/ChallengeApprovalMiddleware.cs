using System;
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
            var token = context.Request.Path.ToString();
            if (token.StartsWith("/"))
            {
                token = token.Substring(1);
            }

            var matchingChallenge = await _store.GetChallengesAsync(token, context.RequestAborted);
            if (matchingChallenge == null)
            {
                _logger.LogInformation(
                    "The given challenge did not match {challengePath}",
                    context.Request.Path.ToString(),
                    token);
                await next(context);
                return;
            }

            _logger.LogInformation("Confirmed challenge request for {token}", token);

            context.Response.ContentLength = matchingChallenge.Response.Length;
            context.Response.ContentType = "application/octet-stream";
            await context.Response.WriteAsync(matchingChallenge.Response, context.RequestAborted);
        }
    }
}
