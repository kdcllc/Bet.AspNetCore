using System.Collections.Generic;

namespace Bet.AspNetCore.ApiKeyAuthentication
{
    public class ApiKeyUser
    {
        public string Email { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public List<string> Roles { get; } = new List<string>();
    }
}
