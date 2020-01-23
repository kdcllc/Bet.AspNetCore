using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.HealthChecks.Publishers;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.UnitTest.HealthChecks
{
    public class HealthCheckPublishersTests
    {
        private readonly ITestOutputHelper _output;

        public HealthCheckPublishersTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task SocketHealthPublisher_Should_Start_TcpListener()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var logFactory = TestLoggerBuilder.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddXunit(_output, LogLevel.Debug);
            });
#pragma warning restore CA2000 // Dispose objects before losing scope

            var port = 8181;

            var publishers = new List<IHealthCheckPublisher>
            {
                new SocketHealthCheckPublisher(port, logFactory.CreateLogger<SocketHealthCheckPublisher>())
            };

            var services = CreateService(publishers.ToArray(), _output);

            var healthService = services.GetRequiredService<HealthCheckService>();

            var report = await healthService.CheckHealthAsync();

            var publisher = services.GetServices<IHealthCheckPublisher>().OfType<SocketHealthCheckPublisher>().Single();

            var response = string.Empty;

            var clientThread = new Thread(() =>
            {
                _output.WriteLine("client-started");

                response = TcpClientResponse(port, "ping");

                _output.WriteLine("client-stopped");
            });

            try
            {
                clientThread.Start();

                _output.WriteLine("server-started");
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Task.Factory.StartNew(async () => await publisher.PublishAsync(report, cts.Token), cts.Token, TaskCreationOptions.None, TaskScheduler.Default);

                Thread.Sleep(TimeSpan.FromSeconds(4));

                Assert.Equal("ping", response);
                Assert.Equal(HealthStatus.Healthy, report.Status);
            }
            finally
            {
                _output.WriteLine("server-stopped");
            }
        }

        [Fact]
        public async Task SocketHealthPublisher_Should_Execute()
        {
            var loggerHelper = new XunitTestOutputHelper();

            var port = 8080;

            var host = new HostBuilder()
                .ConfigureLogging(loggerBuilder =>
                {
                    loggerBuilder.AddXunit(_output, LogLevel.Debug);
                    loggerBuilder.AddXunit(loggerHelper, LogLevel.Debug);
                })
                .ConfigureServices(services =>
                {
                    services.Configure<HealthCheckPublisherOptions>(options =>
                    {
                        options.Delay = TimeSpan.FromSeconds(1);
                        options.Period = TimeSpan.FromSeconds(2);
                        options.Timeout = TimeSpan.FromSeconds(2);
                    });

                    services.AddHealthChecks()
                            .AddCheck("Healthy_Check_One", () => HealthCheckResult.Healthy())
                            .AddCheck("Healthy_Check_Two", () => HealthCheckResult.Healthy())
                            .AddSocketListener(port)
                            .AddLoggerPublisher();
                }).Build();

            await host.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));

            var response1 = TcpClientResponse(port, "ping");
            Assert.Equal("ping", response1);

            var response2 = TcpClientResponse(port, "ping2");
            Assert.Equal("ping2", response2);

            await Task.Delay(TimeSpan.FromSeconds(4));

            await host.StopAsync();
        }

        [Fact]
        public async Task LoggingHealthCheckPublisher_Should_Execute()
        {
            var loggerHelper = new XunitTestOutputHelper();

            var host = new HostBuilder()
                .ConfigureLogging(loggerBuilder =>
                {
                    loggerBuilder.AddXunit(_output, LogLevel.Debug);
                    loggerBuilder.AddXunit(loggerHelper, LogLevel.Debug);
                })
                .ConfigureServices(services =>
                {
                    services.Configure<HealthCheckPublisherOptions>(options =>
                    {
                        options.Delay = TimeSpan.FromSeconds(1);
                        options.Period = TimeSpan.FromSeconds(1);
                        options.Timeout = TimeSpan.FromSeconds(1);
                    });

                    services.AddHealthChecks()
                            .AddCheck("Degraded_Check", () => HealthCheckResult.Degraded())
                            .AddCheck("Unhealthy_Check", () => HealthCheckResult.Unhealthy())
                            .AddLoggerPublisher();
                }).Build();

            await host.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.Contains("Degraded_Check", loggerHelper.Output);
            Assert.Contains("Unhealthy_Check", loggerHelper.Output);

            await host.StopAsync();
        }

        private string TcpClientResponse(int port, string message)
        {
            try
            {
                using var client = new TcpClient("localhost", port);
                using var stream = client.GetStream();
                var data = message.ToBytes();
                stream.Write(data, 0, data.Length);
                data = new byte[256];

                var bytes = stream.Read(data, 0, data.Length);
                var response = data.ConvertToString(0, bytes);
                stream.Close();
                client.Close();

                return response;
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                throw;
            }
        }

        private ServiceProvider CreateService(
            IHealthCheckPublisher[] publishers,
            ITestOutputHelper outputHelper,
            Action<HealthCheckPublisherOptions> configure = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();

            serviceCollection.AddLogging();

            serviceCollection.AddHealthChecks()
                .AddCheck("one", () => HealthCheckResult.Healthy())
                .AddCheck("two", () => HealthCheckResult.Healthy());

            serviceCollection.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromMilliseconds(1);
                options.Period = TimeSpan.FromMilliseconds(1);
                options.Timeout = TimeSpan.FromMilliseconds(1);
            });

            if (publishers != null)
            {
                for (var i = 0; i < publishers.Length; i++)
                {
                    serviceCollection.AddSingleton<IHealthCheckPublisher>(publishers[i]);
                }
            }

            if (configure != null)
            {
                serviceCollection.Configure(configure);
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            var logFactory = TestLoggerBuilder.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddXunit(outputHelper, LogLevel.Debug);
            });
#pragma warning restore CA2000 // Dispose objects before losing scope

            serviceCollection.AddSingleton<ILoggerFactory>(logFactory);

            return serviceCollection.BuildServiceProvider();
        }
    }
}
