using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Bet.AspNetCore.UnitTest
{
    public class KebabRoutingConventionsTests : IDisposable
    {
        private TestServer _server;
        private HttpClient _client;

        public KebabRoutingConventionsTests()
        {
            _server = new TestServer(new WebHostBuilder()
                    .ConfigureServices((context, services) =>
                    {
                       services.AddRouting();
                       services.AddControllers(options => options.Conventions.Add(new KebabRoutingConvention()));
                    }).Configure((app) =>
                    {
                       app.UseRouting();
                       app.UseEndpoints(e => e.MapControllers());
                    }));

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Should_Generate_A_Kebab_Cased_Route()
        {
            var response = await _client.GetAsync("/api/kebab/test-route").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Assert.Equal("success", await response.Content.ReadAsStringAsync());
        }

        public void Dispose()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}
