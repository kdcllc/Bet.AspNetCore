using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace AppAuthentication.Helpers
{
    internal static class ConsoleHandler
    {
        internal static Process OpenBrowser(string url)
        {
            try
            {
                return Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    return Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        internal static string GetUserSecretsId()
        {
            var files = Directory.GetFiles("./", "*.csproj");
            if (files.Length == 0)
            {
                throw new Exception("This command must be run in a project directory");
            }

            var file = XElement.Load(files[0]);
            var groups = file.Elements(XName.Get("PropertyGroup"));
            foreach (var group in groups)
            {
                var secret = group.Element("UserSecretsId");
                if (secret != null)
                {
                    return secret.Value;
                }
            }

            throw new Exception("The UserSecretsId element was not found in your csproj. Please ensure it has been configured.");
        }

        internal static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5050);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
