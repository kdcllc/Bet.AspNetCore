using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Certes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.ML.WebApi.Sample
{
    public class Program
    {
        public const string DomainName = "test.kingdavidconsulting.com";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseLetsEncrypt(configure =>
                        {
                            configure.Email = "info@kingdavidconsulting.com";
                            configure.HostNames = new[] { DomainName };

                            configure.UseStagingServer = true;

                            configure.CertificateFriendlyName = DomainName;
                            configure.CertificatePassword = "7A1FE7EE-8DAF-423D-B43B-A55E6794DCD9";

                            configure.CertificateSigningRequest = new CsrInfo()
                            {
                                CountryName = "US",
                                Locality = "NC",
                                Organization = "KDCLLC",
                                OrganizationUnit = "Dev",
                                CommonName = DomainName
                            };
                        });

                        //webBuilder.ConfigureKestrel(options =>
                        //{
                        //    options.ConfigureHttpsDefaults(configure =>
                        //    {
                        //        configure.ServerCertificate
                        //    });
                        //});

                        webBuilder.UseStartup<Startup>();
                    });
        }
    }
}
