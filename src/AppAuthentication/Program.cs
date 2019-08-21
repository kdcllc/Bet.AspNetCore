using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Console = Colorful.Console;

namespace AppAuthentication
{
    [Command(
        Name = Constants.CLIToolName,
        Description = "Cli tool to help with Docker Container development with Azure MSI Identity.",
        ThrowOnUnexpectedArgument = false,
        AllowArgumentSeparator = true)]
    [Subcommand(typeof(RunCommand))]
    [HelpOption("-?")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            using (var mutex = new Mutex(true, Constants.CLIToolName, out var canCreateNew))
            {
                if (canCreateNew)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw new ApplicationException($"{Constants.CLIToolName} doesn't support this operating system.");
                    }

                    return await CommandLineApplication.ExecuteAsync<Program>(args);
                }
                else
                {
                    Console.WriteLine($"Only one instance of the {Constants.CLIToolName} tool can be run at the same time.", Color.Red);
                    return -1;
                }
            }
        }

        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            Console.WriteAscii(Constants.CLIToolName, Colorful.FigletFont.Default);

            Console.WriteLine("This tool requires to have Visual Studio.NET Installed.", Color.Green);

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
