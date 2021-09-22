using System.Threading.Tasks;

namespace Bet.AspNetCore.ApiKeyAuthentication
{
    public interface IApiUserStore
    {
        Task<ApiKeyUser> GetAsync(string apiKey);
    }
}
