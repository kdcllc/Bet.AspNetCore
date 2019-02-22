using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.UnitTest
{
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddApplicationPart(typeof(TestStartup).Assembly);

            services.AddMvc();

        }

        public void Configure(
           IApplicationBuilder app,
           IHostingEnvironment env)
        {
            app.UseMvc();


            app.UseHealthChecks("/healthy", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckBuilderExtensions.WriteResponse
            });
        }
    }
}
