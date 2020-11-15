using System;
using System.Threading.Tasks;

using Bet.AnalyticsEngine;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Bet.K8s.Web.Sample
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetry();
                })
                .UseSerilog((hostingContext, sp, loggerConfiguration) =>
                {
                    var applicationName = $"BetK8sWeb-{hostingContext.HostingEnvironment.EnvironmentName}";
                    loggerConfiguration
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .AddApplicationInsights(sp);

                    // .AddAzureLogAnalytics(hostingContext.Configuration, applicationName: applicationName);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .CaptureStartupErrors(true)
                        .SuppressStatusMessages(true)
                        .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                        {
                            // based on environment Development = dev; Production = prod prefix in Azure Vault.
                            var envName = hostingContext.HostingEnvironment.EnvironmentName;
                            var configuration = configBuilder.AddAzureKeyVault(
                                hostingEnviromentName: envName,
                                usePrefix: false,
                                reloadInterval: TimeSpan.FromSeconds(30));

                            var keyValue = configuration.GetValue<string>("betk8sweb:testValue");

                            // validation for value the azure key vault
                            if (string.IsNullOrEmpty(keyValue))
                            {
                                throw new ArgumentException("Keyvault failed to retrieve the value");
                            }

                            configuration.DebugConfigurations();
                        })
                        .ConfigureLogging(logging =>
                        {
                            logging.AddConsole();
                            logging.AddDebug();
                        })
                        .ConfigureServices(services =>
                        {
                            services.AnalyticsEngine();

                            services.AddControllers();

                            // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1
                            services.Configure<ForwardedHeadersOptions>(options =>
                            {
                                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                            });

                            services.AddResponseCompression();

                            services.AddHealthChecks()
                                    .AddSigtermCheck("Sigterm_shutdown_check");
                        })
                        .Configure(app =>
                        {
                            app.UseForwardedHeaders();

                            app.UseMiddleware<AnalyticMiddleware>();

                            app.UseRouting();

                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllers();

                                // returns 200 okay
                                endpoints.MapLivenessHealthCheck();

                                // returns healthy if all healthcheks return healthy
                                endpoints.MapHealthyHealthCheck();
                            });

                            app.MapWhen(context => context.Request.Path.Value.Contains("/display"), p =>
                            {
                                p.Run(async c => await DisplayRoute(c));
                            });

                            app.Run(async context => await DefaultRoute(context));
                        });
                });
        }

        private static async Task DefaultRoute(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            // Request method, scheme, and path
            await context.Response.WriteAsync(
                $"Request Method: {context.Request.Method}{Environment.NewLine}");
            await context.Response.WriteAsync(
                $"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
            await context.Response.WriteAsync(
                $"Request Path: {context.Request.Path}{Environment.NewLine}");

            // Headers
            await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");

            foreach (var header in context.Request.Headers)
            {
                await context.Response.WriteAsync($"{header.Key}: " +
                    $"{header.Value}{Environment.NewLine}");
            }

            await context.Response.WriteAsync(Environment.NewLine);

            // Connection: RemoteIp
            await context.Response.WriteAsync(
                $"Request RemoteIp: {context.Connection.RemoteIpAddress}");

            await context.Response.WriteAsync(Environment.NewLine);

            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

            var keyValue = configuration.GetValue<string>("betk8sweb:testValue");

            var storage = context.RequestServices.GetRequiredService<AnalyticsRequestContext>();

            await storage.Database.EnsureCreatedAsync();

            var count = await storage.WebRequest.CountAsync();

            await context.Response.WriteAsync(string.Format("Azure Vault value: {0} - Record Count {1}.", keyValue, count));
        }

        private static async Task DisplayRoute(HttpContext context)
        {
            var storage = context.RequestServices.GetRequiredService<AnalyticsRequestContext>();

            foreach (var item in await storage.WebRequest.AsNoTracking().ToListAsync())
            {
                await context.Response.WriteAsync($"Id: {item.Id}       | Identity: {item.Identity}         | Path: {item.Path}         | Referer: {item.Referer}");

                await context.Response.WriteAsync(Environment.NewLine);
            }
        }
    }
}
