using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OptionsValidationException = Bet.AspNetCore.Options.OptionsValidationException;

namespace Bet.AspNetCore.UnitTest
{
    public class OptionsValidationTests
    {
        [Fact]
        public void Bind_Object_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                {"FakeOptions:Id", "-2" },
                {"FakeOptions:Name", "" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build().GetSection(nameof(FakeOptions));

            var options = new FakeOptions();

            void act() => config.Bind<FakeOptions>(options, opt =>
            {
                if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
                {
                    return true;
                }
                return false;
            }, "Validation Failed");

            Assert.Throws<OptionsValidationException>(act);
        }

        [Fact]
        public void Bind_Object_With_DataAnnotation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                {"FakeOptions:Id", "-2" },
                {"FakeOptions:Name", "" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build().GetSection(nameof(FakeOptions));

            var options = new FakeOptionsWithDatatAnnotations();

            void act() => config.Bind<FakeOptionsWithDatatAnnotations>(options);

            Assert.Throws<OptionsValidationException>(act);
        }

        [Fact]
        public void Configure_With_DataAnnotation_Validation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                {"FakeOptions:Id", "-2" },
                {"FakeOptions:Name", "" }
            };

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                    services.AddMvcCore().AddApplicationPart(typeof(TestStartup).Assembly);
                    services.AddOptions();

                    services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDatatAnnotations>(sectionName: "FakeOptions");
                })
                .Configure(app =>
                {
                    app.UseMvc();
                });

            Assert.Throws<OptionsValidationException>(() => new TestServer(host));
        }

        [Fact]
        public void Configure_With_Validation_Fail()
        {
            var dic = new Dictionary<string, string>
            {
                {"FakeOptions:Id", "-2" },
                {"FakeOptions:Name", "" }
            };

            var host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuiler) =>
                {
                    configBuiler.AddInMemoryCollection(dic).Build();
                })
                .ConfigureServices((services) =>
                {
                    services.AddMvcCore().AddApplicationPart(typeof(TestStartup).Assembly);
                    services.AddOptions();

                    services.ConfigureWithValidation<FakeOptions>(opt =>
                    {
                        if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
                        {
                            return true;
                        }
                        return false;
                    }, "This didn't validated.");
                })
                .Configure(app =>
                {
                    app.UseMvc();
                });

            Assert.Throws<OptionsValidationException>(() => new TestServer(host));
        }
    }
}
