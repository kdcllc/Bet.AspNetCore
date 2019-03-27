using AppAuthentication.AzureCli;
using AppAuthentication.Helpers;
using AppAuthentication.VisualStudio;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Drawing;
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
        public string Authority { get; private set; }

        [Option("-r", Description = "Resource to authenticate against. Provided https://login.microsoftonline.com/{tenantId}. Default set to https://vault.azure.net/")]
        public string Resource { get; private set; }

        [Option("-v", Description = "Allows Verbose logging for the tool. Enable this to get tracing information. The Default is false.")]
        public bool Verbose { get; private set; }

        [Option("-h", Description = "Specify Hosting Environment Name for the cli tool execution. The Default is Development")]
        public string HostingEnviroment { get; private set; }

        [Option("-p", Description = "Specify Web Host port number otherwise it is automatically generated. The Default port if open is 5050.")]
        public int? Port { get; private set; }

        [Option("-c", Description = "Allows to specify a configuration file besides appsettings.json to be specified. The Default appsetting.json located in the execution path.")]
        public string ConfigFile { get; private set; }

        [Option("-t|--token-provider", Description = "The Access Token Provider to retrieve the Authentication Token. The Default provider is VisualStudio.")]
        public TokenProvider TokenProvider { get; } = TokenProvider.VisualStudio;

        [Option("-f|--fix", Description = "Fix command resets Environment Variables.")]
        public bool Fix { get; private set; }

        public string[] RemainingArguments { get; }

        private async Task<int> OnExecuteAsync()
        {
            if (Fix)
            {
                EnvironmentHostedService.ResetVariables();
            }

            var builderConfig = new WebHostBuilderOptions
            {
                Authority = Authority,
                HostingEnvironment = !string.IsNullOrWhiteSpace(HostingEnviroment) ? HostingEnviroment : "Development",
                Resource = !string.IsNullOrWhiteSpace(Resource) ? Resource : "https://vault.azure.net/",
                Verbose = Verbose,
                ConfigFile = ConfigFile,
                SecretId = Guid.NewGuid().ToString()
            };

            try
            {
                builderConfig.Port = Port ?? ConsoleHandler.GetRandomUnusedPort();

                Console.WriteLine($"Active Port: {builderConfig.Port.ToString()}", Color.Green);

                var webHost = WebHostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                                // header: Secret = MSI_SECRET
                                //?resource=clientid=&api-version=2017-09-01
                                .Configure(app =>
                                {
                                    app.Run(async (context) =>
                                    {
                                        var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

                                        var provider = app.ApplicationServices.GetRequiredService<IAccessTokenProvider>();

                                        var requestResource = context.Request.Query["resource"].ToString();

                                        logger.LogDebug("Request QueryString {query}", requestResource);

                                        var resource = !string.IsNullOrWhiteSpace(context.Request.Query["resource"].ToString()) ? requestResource : builderConfig.Resource;

                                        var token = await provider.GetAuthResultAsync(resource, builderConfig.Authority);

                                        var json = JsonConvert.SerializeObject(token);

                                        await context.Response.WriteAsync(json);
                                    });
                                })
                                .ConfigureServices((hostingContext, services) =>
                                {
                                    services.AddSingleton(builderConfig);
                                    services.AddHostedService<EnvironmentHostedService>();

                                    switch (TokenProvider)
                                    {
                                        case TokenProvider.VisualStudio:
                                            services.AddSingleton<IAccessTokenProvider>(sp =>
                                            {
                                                var logger = sp.GetRequiredService<ILogger<ProcessManager>>();
                                                return new VisualStudioAccessTokenProvider(new ProcessManager(logger));
                                            });

                                            break;
                                        case TokenProvider.AzureCli:
                                            services.AddSingleton<IAccessTokenProvider>(sp =>
                                            {
                                                var logger = sp.GetRequiredService<ILogger<ProcessManager>>();
                                                return new AzureCliAccessTokenProvider(new ProcessManager(logger));
                                            });
                                            break;
                                    }
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

    internal enum TokenProvider
    {
        VisualStudio,
        AzureCli
    }
}
