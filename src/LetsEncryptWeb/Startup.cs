using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Certes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LetsEncryptWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLetsEncrypt(
                configure: (options, configuration) =>
                {
                    var domainName = configuration.GetValue<string>("LetsEncryptDomainName");

                    options.HostNames = new[] { domainName };

                    options.CertificateFriendlyName = domainName;

                    options.CertificatePassword = "7a1fe7ee-8daf-423d-b43b-a55e6794dcd9";

                    options.CertificateSigningRequest = new CsrInfo()
                    {
                        CountryName = "US",
                        Organization = "KDCLLC",
                        CommonName = domainName
                    };
                },
                interval: TimeSpan.FromMinutes(5))
                .AddAzureStorage("challenges", "certificates");

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
