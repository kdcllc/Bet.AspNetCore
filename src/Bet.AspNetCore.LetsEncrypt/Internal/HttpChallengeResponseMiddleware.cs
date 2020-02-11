using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Order;
using Bet.Extensions.LetsEncrypt.Order.Stores;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class HttpChallengeResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAcmeChallengeStore _store;
        private readonly ILogger<HttpChallengeResponseMiddleware> _logger;

        public HttpChallengeResponseMiddleware(
            RequestDelegate next,
            IAcmeChallengeStore store,
            ILogger<HttpChallengeResponseMiddleware> logger)
        {
            _next = next;
            _store = store;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // assumes that this middleware has been mapped
            var token = context.Request.Path.ToString();
            if (token.StartsWith("/"))
            {
                token = token.Substring(1);
            }

            var matchingChallenge = await _store.LoadAsync<AcmeChallengeResponse>(token, context.RequestAborted);
            if (matchingChallenge == null)
            {
                _logger.LogInformation(
                    "The given challenge did not match {challengePath}",
                    context.Request.Path.ToString(),
                    token);
                await _next(context);
                return;
            }

            _logger.LogInformation("Confirmed challenge request for {token}", token);

            context.Response.ContentLength = matchingChallenge.Response.Length;
            context.Response.ContentType = "application/octet-stream";
            await context.Response.WriteAsync(matchingChallenge.Response, context.RequestAborted);
        }
    }
}
