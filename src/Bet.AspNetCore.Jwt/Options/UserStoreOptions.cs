using System.Collections.Generic;

using Bet.AspNetCore.Jwt.ViewModels;

namespace Bet.AspNetCore.Jwt.Options
{
    public class UserStoreOptions
    {
        public List<AuthorizeUser> Users { get; set; } = new List<AuthorizeUser>();
    }
}
