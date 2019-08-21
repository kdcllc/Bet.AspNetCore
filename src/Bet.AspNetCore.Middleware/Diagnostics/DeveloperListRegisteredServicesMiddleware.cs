using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.Middleware.Diagnostics
{
    public class DeveloperListRegisteredServicesMiddleware
    {
        private readonly ObjectPool<StringBuilder> _builderPool;
        private readonly RequestDelegate _next;
        private readonly DeveloperListRegisteredServicesOptions _options;

        public DeveloperListRegisteredServicesMiddleware(
            RequestDelegate next,
            IOptions<DeveloperListRegisteredServicesOptions> options,
            ObjectPoolProvider poolProvider)
        {
            _builderPool = poolProvider.CreateStringBuilderPool();
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == _options.Path)
            {
                var builder = _builderPool.Get();
                try
                {
                    var result = string.Empty;

                    switch (_options.PathOutputOptions)
                    {
                        case PathOutputOptions.Html:
                            result = BuildHtml(builder, _options);
                            break;
                        case PathOutputOptions.Json:
                            result = BuildJson(builder, _options);
                            break;
                    }

                    await context.Response.WriteAsync(result);
                }
                finally
                {
                    _builderPool.Return(builder);
                }
            }
            else
            {
                await _next(context);
            }
        }

        private string BuildJson(
            StringBuilder sb,
            DeveloperListRegisteredServicesOptions options)
        {
            sb.Append("[");

            var len = options.Services.Count;
            var i = 0;

            foreach (var svc in options.Services)
            {
                sb.Append("{");
                sb.Append("\"Type\": \"").Append(svc.ServiceType.FullName).Append("\",");
                sb.Append("\"Lifetime\": \"").Append(svc.Lifetime).Append("\",");
                sb.Append("\"Instance\": \"").Append(svc.ImplementationType?.FullName).Append("\"");
                sb.Append("}");
                if (i < len - 1)
                {
                    sb.Append(",");
                }

                i++;
            }

            sb.Append("]");

            return sb.ToString();
        }

        private string BuildHtml(
            StringBuilder sb,
            DeveloperListRegisteredServicesOptions options)
        {
            sb.Append("<h1>All Services</h1>");
            sb.Append("<table><thead>");
            sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
            sb.Append("</thead><tbody>");
            foreach (var svc in options.Services)
            {
                sb.Append("<tr>");
                sb.Append("<td>").Append(svc.ServiceType.FullName).Append("</td>");
                sb.Append("<td>").Append(svc.Lifetime).Append("</td>");
                sb.Append("<td>").Append(svc.ImplementationType?.FullName).Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            return sb.ToString();
        }
    }
}
