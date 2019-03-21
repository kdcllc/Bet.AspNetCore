using System;

namespace Microsoft.AspNetCore.Hosting
{
    public static class Extensions
    {
        public static bool IsDevelopment(this IWebHostEnvironment environment)
        {
            return environment.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
