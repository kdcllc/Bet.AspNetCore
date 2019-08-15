using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

using OptionsValidationException = Bet.Extensions.Options.OptionsValidationException;

namespace Bet.AspNetCore.UnitTest
{
    public class OptionsValidationTests
    {
        [Fact]
        public void Bind_Object_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptions:Id", "-2" },
                { "FakeOptions:Name", string.Empty }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build().GetSection(nameof(FakeOptions));

            var options = new FakeOptions();

            void Act() => config.Bind<FakeOptions>(
                options,
                opt => opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name),
                "Validation Failed");

            var formatted = options.Format();

            Assert.Throws<OptionsValidationException>(Act);
        }

        [Fact]
        public void Bind_Object_With_DataAnnotation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptions:Id", "-2" },
                { "FakeOptions:Name", string.Empty }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build().GetSection(nameof(FakeOptions));

            var options = new FakeOptionsWithDataAnnotations();

            void Act() => config.Bind<FakeOptionsWithDataAnnotations>(options);

            Assert.Throws<OptionsValidationException>(Act);
        }

        [Fact]
        public void Configure_With_DataAnnotation_Validation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptions:Id", "-2" },
                { "FakeOptions:Name", string.Empty }
            };

            IConfiguration configuration = null;

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configuration = configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                    services.AddConfigurationValidation();

                    services.AddMvcCore().AddApplicationPart(typeof(TestStartup).Assembly);
                    services.AddOptions();

                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions");
                })
                .Configure(app =>
                {
                    app.UseMvc();
                });

            Assert.Throws<OptionsValidationException>(() => new TestServer(host));
        }

        [Fact]
        public void Configure_With_DataAnnotation_Validation_Succeded()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptionsWithDataAnnotations:Id", "2" },
                { "FakeOptionsWithDataAnnotations:Name", "bet" },
                { "FakeOptions:Id", "3" },
                { "FakeOptions:Name", "gimel" },
                { "FakeOptions2:Id", "4" },
                { "FakeOptions2:Name", "dalet" }
            };

            IConfiguration configuration = null;

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configuration = configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                    services.AddConfigurationValidation();

                    services.AddMvcCore().AddApplicationPart(typeof(TestStartup).Assembly);
                    services.AddOptions();

                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptionsWithDataAnnotations");

                    // services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(Configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions>(configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions2>(configuration.GetSection("FakeOptions2"));

#if NETCOREAPP3_0
                    services.AddMvc(options => options.EnableEndpointRouting = false);
#endif
                })
                .Configure(app =>
                {
                    app.UseMvc();
                });

            var server = new TestServer(host);

            var result = server.Host.Services.GetService<FakeOptionsWithDataAnnotations>();

            Assert.Equal(2, result.Id);
        }

        [Fact]
        public void Configure_With_Validation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptions:Id", "-2" },
                { "FakeOptions:Name", string.Empty }
            };

            IConfiguration configuration = null;

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configuration = configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                    services.AddConfigurationValidation();

                    services.AddMvcCore().AddApplicationPart(typeof(TestStartup).Assembly);
                    services.AddOptions();

                    services.ConfigureWithValidation<FakeOptions>(
                        configuration,
                        opt => opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name),
                        "This didn't validated.");
                })
                .Configure(app =>
                {
                    app.UseMvc();
                });

            Assert.Throws<OptionsValidationException>(() => new TestServer(host));
        }

        [Fact]
        public void Configure_HostBuilder_With_DataAnnotation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptions:Id", "-2" },
                { "FakeOptions:Name", string.Empty },
                { "FakeOptions2:Id", "-2" },
                { "FakeOptions2:Name", string.Empty }
            };

            IConfiguration configuration = null;

            var host = new HostBuilder()
                .UseStartupFilter()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configuration = configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions");
                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions2");
                })
                .Build();

            void Act() => host.Run();

            // Act();
            Assert.Throws<OptionsValidationException>(() => Act());
        }

        [Fact]
        public async Task Configure_HostBuilder_With_DataAnnotation_Succeeded()
        {
            var dic = new Dictionary<string, string>
            {
                { "FakeOptionsWithDataAnnotations:Id", "2" },
                { "FakeOptionsWithDataAnnotations:Name", "bet" },
                { "FakeOptions:Id", "3" },
                { "FakeOptions:Name", "gimel" },
                { "FakeOptions2:Id", "4" },
                { "FakeOptions2:Name", "dalet" }
            };

            IConfiguration configuration = null;

            var hostBuilder = new HostBuilder()
                .UseStartupFilter()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configuration = configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions>(configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions2>(configuration.GetSection("FakeOptions2"));
                })
                .Build();

            var sp = hostBuilder.Services;
            await hostBuilder.StartAsync();
            var result1 = sp.GetService<FakeOptionsWithDataAnnotations>();

            var result2 = sp.GetService<FakeOptions>();

            Assert.Equal(2, result1.Id);

            await hostBuilder.StopAsync();
        }
    }
}
