using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Bet.AspNetCore.Sample
{
    [SuppressMessage("Readability", "RCS1102", Justification = "Valid entry point to the application.")]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(20));
                            webBuilder.UseStartup<Startup>();
                        });
        }
    }
}
