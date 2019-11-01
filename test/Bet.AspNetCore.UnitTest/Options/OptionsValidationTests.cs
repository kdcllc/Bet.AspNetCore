using System.Collections.Generic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

using OptionsValidationException = Bet.Extensions.Options.OptionsValidationException;

namespace Bet.AspNetCore.UnitTest.Options
{
    public class OptionsValidationTests
    {
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
                    services.AddOptions();

                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions");
                })
                .Configure(app => { });

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
                    services.AddOptions();

                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptionsWithDataAnnotations");

                    // services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(Configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions>(configuration);
                    services.ConfigureWithDataAnnotationsValidation<FakeOptions2>(configuration.GetSection("FakeOptions2"));
                })
                .Configure(app => { });

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
                    services.AddOptions();

                    services.ConfigureWithValidation<FakeOptions>(
                        configuration,
                        opt => opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name),
                        "This didn't validated.");
                })
                .Configure(app => { });

            Assert.Throws<OptionsValidationException>(() => new TestServer(host));
        }
    }
}
