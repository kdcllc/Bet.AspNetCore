using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Xunit;

using OptionsValidationException = Bet.Extensions.Options.OptionsValidationException;

namespace Bet.Extensions.UnitTest.Options
{
    public class OptionsValidationTests
    {
        [Fact]
        public void Bind_Enviroments_Dictionary()
        {
            var dic = new Dictionary<string, string>
            {
                { "Environments:Development", "Development" },
                { "Environments:Staging", "Staging" },
                { "Environments:Production", "Production" }
            };

            var env = new Environments();
            env.Clear();
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();

            config.Bind(nameof(Environments), env);

            Assert.NotNull(env);
        }

        [Fact]
        public void Add_Enviroments_DI()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Environments:Development", "Development" },
                { "Environments:Staging", "Staging" },
                { "Environments:Production", "Production" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();

            services.AddSingleton<IConfiguration>(config);
            services.AddEnvironmentsOptions();
            services.AddEnvironmentsOptions();

            var sp = services.BuildServiceProvider();

            var pr = sp.GetServices<IOptions<Environments>>();
            Assert.Single(pr);

            var env = sp.GetRequiredService<IOptions<Environments>>().Value;

            Assert.Equal(3, env.Count);
        }

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
                .UseStartupFilters()
                .UseOptionValidation()
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
                .UseOptionValidation()
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
