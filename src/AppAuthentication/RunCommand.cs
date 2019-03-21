using AppAuthentication.Helpers;
using AppAuthentication.VisualStudio;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace AppAuthentication
{
    [Command("run",
        Description = "Runs instance of the local server that returns authentication tokens.",
        ThrowOnUnexpectedArgument = false,
        AllowArgumentSeparator = true)]
    internal class RunCommand
    {
        [Option("-a", Description = "Authority Azure TenantId or Azure Directory ID")]
        public string Authority { get; set; }

        [Option("-r", Description = "Resource to authenticate against. Default set to https://vault.azure.net/")]
        public string Resource { get; set; }

        [Option("-v", Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public bool Verbose { get; set; }

        [Option("-h", Description = "Specify Hosting Environment Name for the cli tool execution.")]
        public string HostingEnviroment { get; set; }

        [Option("-p", Description = "Specify Web Host port number otherwise it is automatically generated.")]
        public int? Port { get; set; }

        [Option("-c", Description = "Allows to specify a configuration file besides appsettings.json to be specified.")]
        public string ConfigFile { get; set; }

        public string[] RemainingArguments { get; }

        private async Task<int> OnExecuteAsync()
        {
            var builderConfig = new WebHostBuilderOptions
            {
                Authority = Authority,
                HostingEnviroment = !string.IsNullOrWhiteSpace(HostingEnviroment) ? HostingEnviroment : "Development",
                Resource = !string.IsNullOrWhiteSpace(Resource) ? Resource : "https://vault.azure.net/",
                Verbose = Verbose,
                ConfigFile = ConfigFile
            };

            try
            {
                builderConfig.Port = Port ?? ConsoleHandler.GetRandomUnusedPort();

                Console.WriteLine(builderConfig.Port.ToString(), Color.Green);

                var webHost = WebHostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                                // header: Secret = MSI_SECRET
                                //?resource=clientid=&api-version=2017-09-01
                                .Configure(app =>
                                {
                                    app.Run(async (context) =>
                                    {
                                        var visualStudioProvider = new VisualStudioAccessTokenProvider(new ProcessManager());

                                        var requestResource = context.Request.Query["resource"].ToString();// ?? builderConfig.Resource;
                                        var result = await visualStudioProvider.GetAuthResultAsync(requestResource, builderConfig.Authority);

                                        var json = JsonConvert.SerializeObject(result.token);

                                        await context.Response.WriteAsync(json);
                                    });
                                })
                                .ConfigureServices((hostingContext, services) =>
                                {
                                    services.AddSingleton(builderConfig);
                                    services.AddHostedService<EnvironmentHostedService>();
                                })

                                .Build();

                await webHost.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.Red);
                return -1;
            }
        }
    }
}
