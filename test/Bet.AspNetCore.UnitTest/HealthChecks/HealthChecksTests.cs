using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Xunit;

namespace Bet.AspNetCore.UnitTest.HealthChecks
{
    public class HealthChecksTests
    {
        [Fact]
        public async Task Return_Unhealthy_On_SIGTERM()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks().AddSigtermCheck("sigterm_check");
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/hc");
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();
#if NETCOREAPP2_2
            var appLifetime = server.Host.Services.GetService<Microsoft.AspNetCore.Hosting.IApplicationLifetime>();
#else
            var appLifetime = server.Host.Services.GetService<IHostApplicationLifetime>();
#endif

            appLifetime.StopApplication();

            var response = await client.GetAsync("hc");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task Return_Ok_Status()
        {
            var statusCode = 200;

            var factoryMock = new Mock<IHttpClientFactory>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            });

            factoryMock.Setup(x => x.CreateClient("Successful")).Returns(new HttpClient(fakeHttpMessageHandler));
            var hostBuilder = new WebHostBuilder()
                .Configure(app =>
                {
#if NETCOREAPP2_2
                    app.UseMvc();

                    app.UseHealthChecks("/healthy", new HealthCheckOptions
                    {
                        ResponseWriter = HealthChecksApplicationBuilderExtensions.WriteResponse
                    });
#elif NETCOREAPP3_0
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHealthChecks("/healthy", new HealthCheckOptions
                        {
                            ResponseWriter = HealthChecksApplicationBuilderExtensions.WriteResponse
                        });
                    });
#endif
                })
                .ConfigureServices(services =>
                {
#if NETCOREAPP2_2
                    services.AddMvcCore()
                      .AddApplicationPart(typeof(HealthChecksTests).Assembly);
#elif NETCOREAPP3_0
                    services.AddControllers()
                     .AddApplicationPart(typeof(HealthChecksTests).Assembly);
                    services.AddRouting();
#endif
                    services.AddSingleton<IHttpClientFactory>(factoryMock.Object);

                    services.AddHealthChecks()
                     .AddUriHealthCheck("Successful", builder =>
                     {
                         builder.Add(options =>
                         {
                             options
                             .AddUri($"http://localhost/api/HttpStat/${statusCode}")
                             .UseTimeout(TimeSpan.FromSeconds(60))
                             .UseExpectedHttpCode(statusCode);
                         });
                     });
                });

            var client = new TestServer(hostBuilder).CreateClient();

            var result = await client.GetAsync("/healthy");
            Assert.Equal(statusCode, (int)result.StatusCode);
        }
    }
}
