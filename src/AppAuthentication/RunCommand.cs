using McMaster.Extensions.CommandLineUtils;
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
        public string Authority { get; set; }

        [Option("-r", Description = "Resource to authenticate against. Default set to https://vault.azure.net/")]
        public string Resource { get; set; }

        [Option("-v", Description = "Allows Verbose logging for the tool. Enable this to get tracing information. Default is false.")]
        public bool Verbose { get; set; }

        [Option("-h", Description = "Specify Hosting Environment Name for the cli tool execution.")]
        public string HostingEnviroment { get; set; }

        [Option("-p", Description ="Specify Web Host port number otherwise it is automatically generated.")]
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
                Resource = Resource,
                Verbose = Verbose,
                ConfigFile = ConfigFile
            };

            try
            {
                var webHost = WebHostBuilderExtensions.CreateDefaultBuilder(builderConfig)
                                .ConfigureServices((hostingContext, services) =>
                                {
                                   //TODO register the hosting
                                }).Build();

                await webHost.StartAsync();

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
