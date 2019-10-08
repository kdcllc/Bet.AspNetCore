using System.Threading.Tasks;

using AppAuthentication.Models;

namespace AppAuthentication
{
    internal interface IAccessTokenProvider
    {
        Task<AuthenticationToken> GetAuthResultAsync(string resource, string authority);
    }
}
