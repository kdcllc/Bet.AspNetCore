using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using McMaster.Extensions.CommandLineUtils;

namespace AppAuthentication
{
    public static class IConsoleExtensions
    {
        public static void WriteLine(this IConsole console, ConsoleColor color, string format, params object[] arg)
        {
            var originalColor = console.ForegroundColor;
            console.ForegroundColor = color;
            console.WriteLine(format, arg);
            console.ForegroundColor = originalColor;
        }

        public static void WriteLine(this IConsole console, ConsoleColor color, string format)
        {
            var originalColor = console.ForegroundColor;
            console.ForegroundColor = color;
            console.WriteLine(format);
            console.ForegroundColor = originalColor;
        }
    }
}
