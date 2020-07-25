using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.UnitTest
{
    internal class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(TestStartup).Assembly);
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
