using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using AppAuthentication.VisualStudio;
using McMaster.Extensions.CommandLineUtils;
using Console = Colorful.Console;

namespace AppAuthentication
{
    [Command(Name = "appauthentication",
             Description = "Cli tool to help with Docker Container development with Azure MSI Identity.",
             ThrowOnUnexpectedArgument = false,
             AllowArgumentSeparator =true)]
    [Subcommand(typeof(RunCommand))]
    [HelpOption("-?")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            //var vaultUrl = "https://vault.azure.net/";

            //var vs = new VisualStudioAccessTokenProvider(new ProcessManager());

            //var result = await vs.GetAuthResultAsync(vaultUrl, "");

            return await CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            Console.WriteAscii("AppAuthentication", Colorful.FigletFont.Default);

            Console.WriteLine("You must specify at a subcommand.", Color.Red);
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
        {
            return typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
