using System.Collections.Generic;

namespace Bet.AspNetCore.ApiKeyAuthentication.Options
{
    public class ApiUserStoreOptions
    {
        public List<ApiKeyUser> ApiKeyUsers { get; set; } = new ();
    }
}
