using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AnalyticsEngine
{
    public class AnalyticMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _provider;

        public AnalyticMiddleware(RequestDelegate next, IServiceProvider provider)
        {
            _next = next;
            _provider = provider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Pass the command to the next task in the pipeline
            await _next(context);

            var identity = context.UserIdentity();

            // Let's build our structure with collected data
            var req = new WebRequest
            {
                Timestamp = DateTime.Now,
                Identity = identity,
                RemoteIpAddress = context.Connection.RemoteIpAddress.ToString(),
                Method = context.Request.Method,
                UserAgent = context.Request.Headers["User-Agent"],
                Path = context.Request.Path.Value,
                IsWebSocket = context.WebSockets.IsWebSocketRequest,
                IpCode = context.Connection.RemoteIpAddress.ToFullDecimalString()
            };

            using var scope = _provider.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<AnalyticsRequestContext>();

            await storage.Database.EnsureCreatedAsync();
            storage.WebRequest.Add(req);

            await storage.SaveChangesAsync();
        }
    }
}
