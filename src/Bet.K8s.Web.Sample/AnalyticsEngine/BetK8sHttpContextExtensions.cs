using System.Linq;
using System.Net;
using System.Numerics;

namespace Microsoft.AspNetCore.Http
{
    public static class BetK8sHttpContextExtensions
    {
        private const string CookieName = "SSA_Identity";
        private const string StrFormat = "000000000000000000000000000000000000000";

        public static string UserIdentity(this HttpContext context)
        {
            var identity = context.User?.Identity?.Name;

            if (!context.Request.Cookies.ContainsKey(CookieName))
            {
                if (string.IsNullOrWhiteSpace(identity))
                {
                    identity = context.Request.Cookies.ContainsKey("ai_user")
                                ? context.Request.Cookies["ai_user"]
                                : context.Connection.Id;
                }

                if (!context.Response.HasStarted)
                {
                    context.Response.Cookies.Append("identity", identity);
                }
            }
            else
            {
                identity = context.Request.Cookies[CookieName];
            }

            return identity;
        }

        public static string ToFullDecimalString(this IPAddress ip)
        {
            return new BigInteger(ip.MapToIPv6().GetAddressBytes().Reverse().ToArray()).ToString(StrFormat);
        }
    }
}
